using System;
using System.IO;

using Met.Core.Extensions;

namespace Met.Core.Trans
{
    public class TransportConfig
    {
        private const int URL_SIZE = 512;

        public Uri Uri { get; set; }
        public string Url { get; set; }
        public UInt32 CommsTimeout { get; set; }
        public UInt32 RetryTotal { get; set; }
        public UInt32 RetryWait { get; set; }

        public TransportConfig(BinaryReader reader)
        {
            this.Url = reader.ReadWideString(URL_SIZE);
            this.CommsTimeout = reader.ReadUInt32();
            this.RetryTotal = reader.ReadUInt32();
            this.RetryWait = reader.ReadUInt32();
            this.Uri = new Uri(this.Url);
        }

        public ITransport CreateTransport(Session session)
        {
            switch (this.Uri.Scheme.ToLower())
            {
                case "tcp":
                    {
                        return new TcpTransport(this, session);
                    }
                case "http":
                    {
                        return new HttpTransport(this, session);
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}
