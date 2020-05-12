using Met.Core.Proto;
using System;
using System.Net.Sockets;

namespace Met.Stdapi.Channel
{
    public class TcpClientChannel : Core.Channel
    {
        private readonly string peerHost = null;
        private readonly int peerPort = 0;
        private readonly uint flags = 0;
        private readonly uint channelClass = 0;
        private readonly TcpClient tcpClient = null;
        private readonly NetworkStream stream = null;
        private readonly byte[] readBuffer = null;

        private bool closing = false;

        protected TcpClientChannel(Action<Packet> packetDispatcher, string peerHost, int peerPort, uint flags, uint channelClass, TcpClient tcpClient)
            : base(packetDispatcher)
        {
            this.peerHost = peerHost;
            this.peerPort = peerPort;
            this.flags = flags;
            this.channelClass = channelClass;
            this.tcpClient = tcpClient;
            this.stream = tcpClient.GetStream();
            this.readBuffer = new byte[ushort.MaxValue];

            BeginRead();
        }

        public static TcpClientChannel Create(Action<Packet> packetDispatcher, Packet request, Packet response)
        {
            var peerHost = request.Tlvs[TlvType.StdapiPeerHost][0].ValueAsString();
            var peerPort = (int)request.Tlvs[TlvType.StdapiPeerPort][0].ValueAsDword();
            var flags = request.Tlvs[TlvType.Flags][0].ValueAsDword();
            var channelClass = request.Tlvs[TlvType.Flags][0].ValueAsDword();

            var tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(peerHost, peerPort);

                return new TcpClientChannel(packetDispatcher, peerHost, peerPort, flags, channelClass, tcpClient);
            }
            catch
            {
                return null;
            }
        }

        public override PacketResult Write(Packet request, Packet response)
        {
            var data = request.Tlvs[TlvType.ChannelData][0].ValueAsRaw();
            try
            {
                this.stream.Write(data, 0, data.Length);
                response.Add(TlvType.Length, data.Length);
                return PacketResult.Success;
            }
            catch
            {
                // TODO: add a proper result
                return PacketResult.InvalidData;
            }
        }

        private void BeginRead()
        {
            this.stream.BeginRead(this.readBuffer, 0, this.readBuffer.Length, SocketReadCompleted, this);
        }

        private static void SocketReadCompleted(IAsyncResult result)
        {
            var connectionClosed = false;

            TcpClientChannel channel = (TcpClientChannel)result.AsyncState;
            try
            {
                int bytesRead = channel.stream.EndRead(result);

                if (bytesRead > 0)
                {
                    var packet = new Packet("core_channel_write");
                    packet.Add(TlvType.ChannelId, channel.ChannelId);
                    packet.Add(TlvType.ChannelData, channel.readBuffer, bytesRead);

                    channel.PacketDispatcher(packet);
                }
                else
                {
                    connectionClosed = true;
                }

            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("TcpClientChannel Exception: {0}", e.Message));
                connectionClosed = true;
            }

            if (connectionClosed)
            {
                if (!channel.closing)
                {
                    channel.FireClosedEvent();
                    channel.Close();
                }
            }
            else
            {
                channel.BeginRead();
            }
        }

        public override void Close()
        {
            this.closing = true;
            this.tcpClient.Close();
        }
    }
}
