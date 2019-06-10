using Met.Core.Trans;
using System.Collections.Generic;
using System.IO;
using System;
using Met.Core.Extensions;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using Met.Core.Proto;

namespace Met.Core
{
    public class Server
    {
        private ITransport currentTransport;
        private int transportIndex;
        private PluginManager pluginManager;
        private CommandHandler commandHandler;

        private Session Session { get; set; }
        private List<ITransport> Transports { get; set; }

        private Server()
        {
            this.pluginManager = new PluginManager(this.DispatchPacket);
            this.Transports = new List<ITransport>();
            this.commandHandler = new CommandHandler();

            this.commandHandler.Register(this.pluginManager);
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

        public static void Bootstrap(BinaryReader reader, TcpClient tcpClient)
        {
            var metSrv = new Server(reader);
            metSrv.Run(tcpClient);
        }

        public static void Bootstrap(BinaryReader reader)
        {
            var metSrv = new Server(reader);
            metSrv.Run();
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
            var running = true;
            try
            {
                RegisterServerCommands();

                while (running)
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

                    switch (PacketDispatchLoop())
                    {
                        case InlineProcessingResult.Shutdown:
                            {
                                running = false;
                                break;
                            }
                        case InlineProcessingResult.NextTransport:
                            {
                                // TODO: change transports
                                break;
                            }
                        case InlineProcessingResult.Continue:
                        default:
                            {
                                // TODO: remove this case down the track
                                break;
                            }
                    }
                }
            }
            catch (TimeoutException)
            {
                // the session has timed out, clean up and shut down
            }

            foreach (var transport in this.Transports)
            {
                transport.Dispose();
            }

            this.Transports.Clear();
        }

        private void DispatchPacket(Packet packet)
        {
            this.currentTransport.SendPacket(packet);
        }

        private void RegisterServerCommands()
        {
            this.pluginManager.RegisterFunction(string.Empty, "core_shutdown", true, CoreShutdown);
        }

        private InlineProcessingResult CoreShutdown(Packet request, Packet response)
        {
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Shutdown;
        }

        private InlineProcessingResult PacketDispatchLoop()
        {
            while (true)
            {
                var request = this.currentTransport.ReceivePacket();
                if (request != null)
                {
                    var response = request.CreateResponse();
                    response.Add(TlvType.Uuid, this.Session.SessionUuid);
                    var result = this.pluginManager.InvokeHandler(request, response);

                    if (result != InlineProcessingResult.Continue)
                    {
                        return result;
                    }
                }
                else
                {
                    return InlineProcessingResult.NextTransport;
                }
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
                var transport = transportConfig.CreateTransport(this.Session);
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
