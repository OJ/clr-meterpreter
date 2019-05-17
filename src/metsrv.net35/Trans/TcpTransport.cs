using System;
using System.IO;
using System.Net.Sockets;
using Met.Core.Proto;

namespace Met.Core.Trans
{
    public class TcpTransport : ITransport
    {
        private TcpClient tcpClient;
        private NetworkStream tcpStream;
        private BinaryReader tcpReader;
        private Session session;

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
            this.Config = config;
            this.session = session;
        }

        public void Configure(BinaryReader reader)
        {
            // We don't need to get any other configuration out in TCP transports.
        }

        public bool Connect()
        {
            if (!this.IsConnected)
            {
                var client = new TcpClient();
                client.Connect(this.Config.Uri.Host, this.Config.Uri.Port);
                if (client.Connected)
                {
                    this.Wrap(client);
                }
            }

            return this.IsConnected;
        }

        public void Wrap(TcpClient tcpClient)
        {
            Dispose();
            this.tcpClient = tcpClient;
            this.tcpStream = this.tcpClient.GetStream();
            this.tcpReader = new BinaryReader(this.tcpStream);
        }

        public void Dispose()
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

        public Packet ReceivePacket()
        {
            return new Packet(this.tcpReader);
        }

        public void SendPacket(Packet response)
        {
            var rawPacket = response.ToRaw(this.session.SessionGuid);
            this.tcpStream.Write(rawPacket, 0, rawPacket.Length);
        }
    }
}
