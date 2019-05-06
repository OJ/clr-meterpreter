using System;

namespace Met.Core.Trans
{
    public class TcpTransport : ITransport
    {
        public TransportConfig Config { get; private set; }

        public TcpTransport(TransportConfig config)
        {
            this.Config = config;
        }
    }
}
