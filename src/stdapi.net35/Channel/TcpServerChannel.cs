using Met.Core.Extensions;
using System;
using Met.Core.Proto;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Met.Stdapi.Channel
{
    public class TcpServerChannel : Core.Channel
    {
        private readonly IPAddress bindAddress;
        private readonly int localPort;
        private readonly TcpListener listener;

        public int LocalPort
        {
            get { return localPort; }
        }

        public string LocalHost
        {
            get { return bindAddress.ToString(); }
        }

        private TcpServerChannel(Core.ChannelManager channelManager, string localHost, int localPort)
            : base(channelManager)
        {
            this.localPort = localPort;
            this.bindAddress = IPAddress.Any;

            if (!string.IsNullOrEmpty(localHost))
            {
                this.bindAddress = IPAddress.Parse(localHost);
            }

            this.listener = new TcpListener(this.bindAddress, this.localPort);
            this.listener.ExclusiveAddressUse = false;
            this.listener.Start();
            BeginAcceptConnection();
        }

        private void BeginAcceptConnection()
        {
            this.listener.BeginAcceptTcpClient(this.TcpClientConnected, null);
        }

        private void TcpClientConnected(IAsyncResult result)
        {
            var client = this.listener.EndAcceptTcpClient(result);
            BeginAcceptConnection();

            if (client != null)
            {
                var clientChannel = TcpClientChannel.Wrap(this.ChannelManager, client);
                this.ChannelManager.Manage(clientChannel);

                var packet = new Packet("tcp_channel_open");
                packet.Add(TlvType.ChannelId, clientChannel.ChannelId);
                packet.Add(TlvType.ChannelParentId, this.ChannelId);
                packet.Add(TlvType.StdapiLocalHost, this.LocalHost);
                packet.Add(TlvType.StdapiLocalPort, this.LocalPort);
                packet.Add(TlvType.StdapiPeerHost, clientChannel.PeerHost);
                packet.Add(TlvType.StdapiPeerPort, clientChannel.PeerPort);
                this.ChannelManager.Dispatch(packet);
            }
        }

        public static TcpServerChannel Create(Core.ChannelManager channelManager, Packet request, Packet response)
        {
            var localHost = request.Tlvs.TryGetTlvValueAsString(TlvType.StdapiLocalHost);
            var localPort = (int)request.Tlvs.TryGetTlvValueAsDword(TlvType.StdapiLocalPort);

            var channel = new TcpServerChannel(channelManager, localHost, localPort);

            response.Add(TlvType.StdapiLocalHost, channel.LocalHost);
            response.Add(TlvType.StdapiLocalPort, channel.LocalPort);

            return channel;
        }

        public override void Close()
        {
            this.listener.Stop();
        }

        public override PacketResult Write(Packet request, Packet response, out int bytesWritten)
        {
            // We are not going to do anything
            bytesWritten = 0;
            return PacketResult.CallNotImplemented;
        }
    }
}
