using Met.Core.Extensions;
using Met.Core.Proto;
using System;
using System.IO;

namespace Met.Core.Trans
{
    public class HttpTransport : ITransport
    {
        private const int UA_SIZE = 256;
        private const int PROXY_HOST_SIZE = 128;
        private const int PROXY_USER_SIZE = 64;
        private const int PROXY_PASS_SIZE = 64;
        private const int CERT_HASH_SIZE = 20;

        private Session session;
        private System.Net.WebClient webClient;

        public TransportConfig Config { get; private set; }
        public string ProxyHost { get; private set; }
        public string ProxyUser { get; private set; }
        public string ProxyPass { get; private set; }
        public string UserAgent { get; private set; }
        public byte[] CertHash { get; private set; }
        public string CustomHeaders { get; private set; }

        public bool IsConnected => true;

        public HttpTransport(TransportConfig config, Session session)
        {
            this.Config = config;
            this.session = session;
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
            // Always true
            return this.IsConnected;
        }

        public void Dispose()
        {
        }

        public void Wrap(System.Net.WebClient webClient)
        {
            this.webClient = webClient;
        }

        public Packet ReceivePacket(PacketEncryptor packetEncryptor)
        {
            var tlvData = this.webClient.DownloadData(this.Config.Uri);
            var delay = 0;
            var failCount = 0;

            while (tlvData.Length == 0)
            {
                delay = 10 * failCount;
                ++failCount;
                System.Threading.Thread.Sleep(Math.Min(10000, delay));
                tlvData = this.webClient.DownloadData(this.Config.Uri);
            }

            using (var tlvStream = new MemoryStream(tlvData))
            using (var reader = new BinaryReader(tlvStream))
            {
                return new Packet(reader, packetEncryptor);
            }
        }

        public void SendPacket(byte[] responsePacket)
        {
            var wc = new System.Net.WebClient();
            wc.UploadData(this.Config.Uri, responsePacket);
        }
    }
}
