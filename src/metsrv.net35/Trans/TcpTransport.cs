using Met.Core.Proto;
using System;
using System.IO;
using System.Net.Sockets;

namespace Met.Core.Trans
{
    public class TcpTransport : ITransport
    {
        private TcpClient tcpClient = null;
        private NetworkStream tcpStream = null;
        private BinaryReader tcpReader = null;
        private Session session = null;
        private object tcpSendLock = null;

        public TransportConfig Config { get; private set; }

        public bool IsConnected
        {
            get
            {
                return this.tcpClient != null && this.tcpClient.Connected;
            }
        }

        public TcpTransport(TransportConfig config, Session session)
        {
            this.tcpSendLock = new object();
            this.Config = config;
            this.session = session;
        }

        public void Configure(BinaryReader reader)
        {
            // We don't need to get any other configuration out in TCP transports.
        }

        public void Configure(Packet request)
        {
            // We don't need to get any other configuration out in TCP transports.
        }

        public bool Connect()
        {
            var client = default(TcpClient);
            if (!this.IsConnected)
            {
                if (this.Config.Uri.Host == System.Net.IPAddress.Any.ToString())
                {
                    var tcpListener = new TcpListener(System.Net.IPAddress.Any, this.Config.Uri.Port);
                    tcpListener.Start(1);
                    client = tcpListener.AcceptTcpClient();
                    tcpListener.Stop();
                }
                else
                {
                    client = new TcpClient();

                    try
                    {
                        client.Connect(this.Config.Uri.Host, this.Config.Uri.Port);
                    }
                    catch
                    {
                        // something went wrong connecting, so assume we haven't succeeded
                        // and just move on with the transport retry/handle functionality
                    }
                }
            }

            if (client != null && client.Connected)
            {
                this.Wrap(client);
            }

            return this.IsConnected;
        }
        
        public void Disconnect()
        {
            if (this.tcpReader != null)
            {
                ((IDisposable)this.tcpReader).Dispose();
                this.tcpReader = null;
            }

            if (this.tcpStream != null)
            {
                this.tcpStream.Close();
                this.tcpStream.Dispose();
                this.tcpStream = null;
            }

            if (this.tcpClient != null)
            {
                ((IDisposable)this.tcpClient).Dispose();
                this.tcpClient = null;
            }
        }

        public void Wrap(TcpClient tcpClient)
        {
            Disconnect();
            this.tcpClient = tcpClient;
            this.tcpStream = this.tcpClient.GetStream();
            this.tcpReader = new BinaryReader(this.tcpStream);
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void GetConfig(ITlv tlv)
        {
            this.Config.GetConfig(tlv);
        }

        public Packet ReceivePacket(PacketEncryptor packetEncryptor)
        {
            try
            {
                var peekBuffer = new byte[4];
                if (this.tcpClient.Client.Receive(peekBuffer, SocketFlags.Peek) == peekBuffer.Length)
                {
                    if (peekBuffer[peekBuffer.Length - 1] == 0)
                    {
                        FlushStage();
                    }
                }

                return new Packet(this.tcpReader, packetEncryptor);
            }
            catch
            {
                // The transport may have bailed while we were trying to read, so return null
                // to indicate a transport error.
            }
            return null;
        }

        public void SendPacket(byte[] responsePacket)
        {
            lock (this.tcpSendLock)
            {
                this.tcpStream.Write(responsePacket, 0, responsePacket.Length);
            }
        }

        private void FlushStage()
        {
            var size = this.tcpReader.ReadInt32();
            this.tcpReader.ReadBytes(size);
        }
    }
}
