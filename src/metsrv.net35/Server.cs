using Met.Core.Trans;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

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
            LoadExtensions(reader);
            LoadExtensionInitialisations(reader);
        }

        private void LoadTransports(BinaryReader reader)
        {
            while (reader.PeekChar() != 0)
            {
                var transportConfig = new TransportConfig(reader);
                this.Transports.Add(transportConfig.CreateTransport());
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
        }
    }
}
