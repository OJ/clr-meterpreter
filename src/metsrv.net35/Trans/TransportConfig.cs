using Met.Core.Extensions;
using Met.Core.Proto;
using System;
using System.IO;

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
            : this(reader.ReadWideString(URL_SIZE),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32())
        {
        }

        public TransportConfig(string url, uint commsTimeout, uint retryTotal, uint retryWait)
        {
            this.Url = url;
            this.CommsTimeout = commsTimeout;
            this.RetryTotal = retryTotal;
            this.RetryWait = retryWait;

            try
            {
                this.Uri = new Uri(this.Url);
            }
            catch (UriFormatException)
            {
                var p = this.Url.Split(':');
                url = string.Format("{0}:{1}{2}:{3}", p[0], p[1], System.Net.IPAddress.Any, p[2]);
                this.Uri = new Uri(url);
            }
        }

        public void GetConfig(ITlv tlv)
        {
            tlv.Add(TlvType.TransUrl, this.Url);
            tlv.Add(TlvType.TransCommTimeout, CommsTimeout);
            tlv.Add(TlvType.TransRetryTotal, RetryTotal);
            tlv.Add(TlvType.TransRetryWait, RetryWait);
        }

        public ITransport CreateTransport(Session session)
        {
            switch (this.Uri.Scheme.ToLowerInvariant())
            {
                case "tcp":
                    {
                        return new TcpTransport(this, session);
                    }
                case "http":
                case "https":
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
