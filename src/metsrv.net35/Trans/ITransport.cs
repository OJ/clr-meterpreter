using Met.Core.Proto;
using System;
using System.IO;

namespace Met.Core.Trans
{
    public interface ITransport : IDisposable
    {
        TransportConfig Config { get; }
        bool IsConnected { get; }

        void Configure(BinaryReader reader);
        bool Connect();
        void Disconnect();

        Packet ReceivePacket(PacketEncryptor packetEncryptor);
        void SendPacket(byte[] responsePacket);
    }
}
