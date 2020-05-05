using Met.Core.Proto;
using System.Net.Sockets;

namespace Met.Stdapi.Channel
{
    public class TcpClientChannel : Core.Channel
    {
        private readonly string peerHost;
        private readonly int peerPort;
        private readonly uint flags;
        private readonly uint channelClass;
        private readonly TcpClient tcpClient;

        protected TcpClientChannel(string peerHost, int peerPort, uint flags, uint channelClass, TcpClient tcpClient)
        {
            this.peerHost = peerHost;
            this.peerPort = peerPort;
            this.flags = flags;
            this.channelClass = channelClass;
            this.tcpClient = tcpClient;
        }

        public static TcpClientChannel Create(Packet request, Packet response)
        {
            var peerHost = request.Tlvs[TlvType.StdapiPeerHost][0].ValueAsString();
            var peerPort = (int)request.Tlvs[TlvType.StdapiPeerPort][0].ValueAsDword();
            var flags = request.Tlvs[TlvType.Flags][0].ValueAsDword();
            var channelClass = request.Tlvs[TlvType.Flags][0].ValueAsDword();

            var tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(peerHost, peerPort);

                return new TcpClientChannel(peerHost, peerPort, flags, channelClass, tcpClient);
            }
            catch
            {
                return null;
            }
        }
    }
}
