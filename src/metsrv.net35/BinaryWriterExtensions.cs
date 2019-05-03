using System;
using System.IO;
using System.Net;
using System.Text;

namespace Met.Core
{
    public static class BinaryWriterExtensions
    {
        public static void WritePacketType(this BinaryWriter writer, PacketType type)
        {
            writer.WriteDword((UInt32)type);
        }

        public static void WriteTlvType(this BinaryWriter writer, TlvType type)
        {
            writer.WriteDword((UInt32)type);
        }

        public static void WriteDword(this BinaryWriter writer, UInt32 value)
        {
            writer.Write((UInt32)IPAddress.HostToNetworkOrder((int)value));
        }

        public static void WriteDword(this BinaryWriter writer, int value)
        {
            writer.Write((UInt32)IPAddress.HostToNetworkOrder(value));
        }

        public static void WriteQword(this BinaryWriter writer, UInt64 value)
        {
            writer.Write((UInt64)IPAddress.HostToNetworkOrder((long)value));
        }

        public static void WriteString(this BinaryWriter writer, string value)
        {
            writer.Write(Encoding.UTF8.GetBytes(value + "\0"));
        }
    }
}
