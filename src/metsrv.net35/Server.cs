using Met.Core.Trans;
using System.Collections.Generic;
using System.IO;

namespace Met.Core
{
    public class Server
    {
        private Session Session { get; set; }
        private List<ITransport> Transports { get; set; }

        private Server()
        {
            this.Transports = new List<ITransport>();
        }

        public Server(BinaryReader reader)
            : this()
        {
            this.Session = new Session(reader);
            LoadTransports(reader);
        }

        private void LoadTransports(BinaryReader reader)
        {
            while (reader.PeekChar() != 0)
            {
                var transportConfig = new TransportConfig(reader);
                this.Transports.Add(transportConfig.CreateTransport());
            }
        }
    }
}
