using Met.Core.Proto;

namespace Met.Core
{
    public interface IPacketDispatcher
    {
        void DispatchPacket(Packet packet);
        void DispatchPacket(byte[] rawPacket);
    }
}
