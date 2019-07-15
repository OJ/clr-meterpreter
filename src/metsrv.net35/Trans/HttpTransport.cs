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

        public Packet ReceivePacket(PacketEncryptor packetEncryptor)
        {
            throw new NotImplementedException();
        }

        public void SendPacket(byte[] responsePacket)
        {
            throw new NotImplementedException();
        }
    }
}
