using Met.Core.Trans;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using Met.Core.Extensions;
using System.Linq;
using System.Threading;
using System.Net.Sockets;

namespace Met.Core
{
    public class Server
    {
        private ITransport currentTransport;
        private int transportIndex;

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
            LoadExtensions(reader);
            LoadExtensionInitialisations(reader);

            this.currentTransport = this.Transports.First();
            this.transportIndex = 0;
        }

        public void Run(TcpClient tcpClient)
        {
            var transport = this.currentTransport as TcpTransport;

            if (transport != null)
            {
                transport.Wrap(tcpClient);
            }

            Run();
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    var transportExpiry = DateTime.UtcNow.AddSeconds(this.currentTransport.Config.RetryTotal);

                    // Make sure that this transport retry timeout has not expired
                    while (transportExpiry > DateTime.UtcNow && !this.currentTransport.Connect())
                    {
                        // Sleep for the requisite timeout between reconnect attempts
                        Thread.Sleep((int)this.currentTransport.Config.RetryWait + 1000);
                        CheckSessionExpiry();
                    }

                    if (!this.currentTransport.IsConnected)
                    {
                        this.transportIndex = (this.transportIndex + 1) % this.Transports.Count;
                        this.currentTransport = this.Transports[this.transportIndex];
                        continue;
                    }

                    var packet = this.currentTransport.GetPacket();
                }
            }
            catch (TimeoutException)
            {
                // the session has timed out, clean up and shut down
            }
        }

        private void CheckSessionExpiry()
        {
            if (DateTime.UtcNow > this.Session.Expiry)
            {
                throw new TimeoutException("Session has expired");
            }
        }

        private void LoadTransports(BinaryReader reader)
        {
            while (reader.PeekChar() != 0)
            {
                var transportConfig = new TransportConfig(reader);
                var transport = transportConfig.CreateTransport();
                transport.Configure(reader);
                this.Transports.Add(transport);
            }

            // Skip the terminating \x00\x00 at the end of the transport list
            reader.ReadUInt16();
        }

        private void LoadExtensions(BinaryReader reader)
        {
            var size = reader.ReadUInt32();

            while (size != 0u)
            {
                var extensionBytes = reader.ReadBytes((int)size);
                LoadExtension(extensionBytes);
                size = reader.ReadUInt32();
            }
        }

        private void LoadExtension(byte[] extensionBytes)
        {
            // TODO: implement this
        }

        private void LoadExtensionInitialisations(BinaryReader reader)
        {
            var extName = reader.ReadNullTerminatedString();
            while (extName.Length > 0)
            {
                var length = reader.ReadUInt32();
                var initContent = reader.ReadBytes((int)length);

                // TODO: get a reference to the extension
                // and pass in the init source
                // var extension = this.GetExtension(extName);
                // extension.Init(initContent);

                extName = reader.ReadNullTerminatedString();
            }
#if THISISNTATHING
            var b = reader.ReadByte();

            while (b != 0)
            {
                var stringBytes = new List<byte>();

                do
                {
                    stringBytes.Add(b);
                    b = reader.ReadByte();
                }
                while (b != 0);

                var extName = Encoding.ASCII.GetString(stringBytes.ToArray());
                var length = reader.ReadUInt32();
                var initContent = reader.ReadBytes((int)length);

                // TODO: get a reference to the extension
                // and pass in the init source
                // var extension = this.GetExtension(extName);
                // extension.Init(initContent);

                // check to see if we have reached the end
                b = reader.ReadByte();
            }
#endif
        }
    }
}
