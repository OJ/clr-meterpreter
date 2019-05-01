using System;
using System.IO;
using System.Net;

namespace Met.Core
{
    public static class BinaryReaderExtensions
    {
        public static bool IsFinished(this BinaryReader reader)
        {
            return reader.PeekChar() == -1;
        }

        public static UInt32 ReadDword(this BinaryReader reader)
        {
            return (UInt32)IPAddress.NetworkToHostOrder((int)reader.ReadUInt32());
        }

        public static UInt64 ReadQword(this BinaryReader reader)
        {
            return (UInt64)IPAddress.NetworkToHostOrder((long)reader.ReadUInt64());
        }

        public static TlvType ReadTlvType(this BinaryReader reader)
        {
            return (TlvType)reader.ReadDword();
        }

        public static PacketType ReadPacketType(this BinaryReader reader)
        {
            return (PacketType)reader.ReadDword();
        }
    }
}
