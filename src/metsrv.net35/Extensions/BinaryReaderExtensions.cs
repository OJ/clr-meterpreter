using System;
using System.IO;
using System.Net;
using System.Text;

using Met.Core.Proto;

namespace Met.Core.Extensions
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

        public static string ReadString(this BinaryReader reader, int length)
        {
            return Encoding.UTF8.GetString(reader.ReadBytes(length)).TrimEnd('\0');
        }

        public static string ReadWideString(this BinaryReader reader, int length)
        {
            return Encoding.Unicode.GetString(reader.ReadBytes(length * 2)).TrimEnd('\0');
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
