using Met.Core.Extensions;
using Met.Core.Proto;
using Met.Core.Trans;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Met.Core
{
    public class Server
    {
        private ITransport currentTransport = null;
        private int transportIndex = 0;
        private PluginManager pluginManager = null;
        private CommandHandler commandHandler = null;
        private PacketEncryptor packetEncryptor = null;

        private Session Session { get; set; }
        private List<ITransport> Transports { get; set; }

        private Server()
        {
            this.pluginManager = new PluginManager(this.DispatchPacket);
            this.Transports = new List<ITransport>();
            this.commandHandler = new CommandHandler();
            this.packetEncryptor = new PacketEncryptor();

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

        public static void Bootstrap(BinaryReader reader, System.Net.WebClient webClient)
        {
            var metSrv = new Server(reader);
            metSrv.Run(webClient);
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

        public void Run(System.Net.WebClient webClient)
        {
            var transport = this.currentTransport as HttpTransport;

            if (transport != null)
            {
                transport.Wrap(webClient);
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
                        Thread.Sleep((int)this.currentTransport.Config.RetryWait * 1000);
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
                                this.currentTransport.Disconnect();
                                break;
                            }
                        case InlineProcessingResult.NextTransport:
                            {
                                this.currentTransport.Disconnect();
                                this.transportIndex = (this.transportIndex + 1) % this.Transports.Count;
                                this.currentTransport = this.Transports[this.transportIndex];
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
            var rawPacket = packet.ToRaw(this.Session.SessionGuid, this.packetEncryptor);

            this.currentTransport.SendPacket(rawPacket);

            if (this.packetEncryptor.HasAesKey && !this.packetEncryptor.Enabled)
            {
                this.packetEncryptor.Enabled = true;
            }
        }

        private void RegisterServerCommands()
        {
            this.pluginManager.RegisterFunction(string.Empty, "core_shutdown", true, this.CoreShutdown);
            this.pluginManager.RegisterFunction(string.Empty, "core_negotiate_tlv_encryption", false, this.CoreNegotiateTlvEncryption);
            this.pluginManager.RegisterFunction(string.Empty, "core_transport_set_timeouts", false, this.TransportSetTimeouts);
            this.pluginManager.RegisterFunction(string.Empty, "core_transport_list", false, this.TransportList);
            this.pluginManager.RegisterFunction(string.Empty, "core_get_session_guid", false, this.CoreGetSessionGuid);
            this.pluginManager.RegisterFunction(string.Empty, "core_set_session_guid", false, this.CoreSetSessionGuid);
            this.pluginManager.RegisterFunction(string.Empty, "core_set_uuid", false, this.CoreSetUuid);
        }

        private InlineProcessingResult TransportList(Packet request, Packet response)
        {
            response.Add(TlvType.TransSessExp, (uint)(this.Session.Expiry - DateTime.UtcNow).TotalSeconds);

            foreach (var transport in this.Transports)
            {
                transport.GetConfig(response.AddGroup(TlvType.TransGroup));
            }

            response.Result = PacketResult.Success;

            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult TransportSetTimeouts(Packet request, Packet response)
        {
            var tlvs = default(List<Tlv>);

            if (request.Tlvs.TryGetValue(TlvType.TransSessExp, out tlvs) && tlvs.Count > 0)
            {
                this.Session.Expiry = DateTime.UtcNow.AddSeconds(tlvs[0].ValueAsDword());
            }

            if (request.Tlvs.TryGetValue(TlvType.TransCommTimeout, out tlvs) && tlvs.Count > 0)
            {
                this.currentTransport.Config.CommsTimeout = tlvs[0].ValueAsDword();
            }

            if (request.Tlvs.TryGetValue(TlvType.TransRetryTotal, out tlvs) && tlvs.Count > 0)
            {
                this.currentTransport.Config.RetryTotal = tlvs[0].ValueAsDword();
            }

            if (request.Tlvs.TryGetValue(TlvType.TransRetryWait, out tlvs) && tlvs.Count > 0)
            {
                this.currentTransport.Config.RetryWait = tlvs[0].ValueAsDword();
            }

            response.Add(TlvType.TransSessExp, (uint)(this.Session.Expiry - DateTime.UtcNow).TotalSeconds);
            this.currentTransport.Config.GetConfig(response);

            response.Result = PacketResult.Success;

            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult CoreNegotiateTlvEncryption(Packet request, Packet response)
        {
            var pubKey = request.Tlvs[TlvType.RsaPubKey].First().ValueAsString();
            var key = this.packetEncryptor.GenerateNewAesKey();

            try
            {
                var encryptedKey = this.packetEncryptor.RsaEncrypt(pubKey, key);
                response.Add(TlvType.EncSymKey, encryptedKey);
            }
            catch
            {
                response.Add(TlvType.SymKey, key);
            }

            response.Add(TlvType.SymKeyType, PacketEncryptor.ENC_AES256);

            this.packetEncryptor.AesKey = key;

            response.Result = PacketResult.Success;

            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult CoreShutdown(Packet request, Packet response)
        {
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Shutdown;
        }
        
        private InlineProcessingResult CoreGetSessionGuid(Packet request, Packet response)
        {
            response.Add(TlvType.SessionGuid, this.Session.SessionGuid);
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }
        
        private InlineProcessingResult CoreSetSessionGuid(Packet request, Packet response)
        {
            this.Session.SessionGuid = request.Tlvs[TlvType.SessionGuid][0].ValueAsRaw();
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult CoreSetUuid(Packet request, Packet response)
        {
            this.Session.SessionUuid = request.Tlvs[TlvType.Uuid][0].ValueAsRaw();
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult PacketDispatchLoop()
        {
            while (true)
            {
                CheckSessionExpiry();
                var request = this.currentTransport.ReceivePacket(this.packetEncryptor);
                if (request != null)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(string.Format("Request: {0}", request.Method));
#endif
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
