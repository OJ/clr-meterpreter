using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Met.Core.Proto;
using System.Collections.Generic;

namespace Met.Core.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static bool IsFinished(this BinaryReader reader)
        {
            return reader.PeekChar() == -1;
        }

        public static UInt16 ReadWord(this BinaryReader reader)
        {
            return (UInt16)IPAddress.NetworkToHostOrder((short)reader.ReadUInt16());
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

        public static IEnumerable<byte> AsByteStream(this BinaryReader reader)
        {
            while (!reader.IsFinished())
            {
                yield return reader.ReadByte();
            }
        }

        public static IEnumerable<ushort> AsUshortStream(this BinaryReader reader)
        {
            while (!reader.IsFinished())
            {
                yield return reader.ReadUInt16();
            }
        }

        public static string ReadNullTerminatedWideString(this BinaryReader reader)
        {
            var wideChars = reader.AsUshortStream()
                .TakeWhile(c => c != 0)
                .Select(c => new List<byte> { (byte)c, (byte)(c >> 8) })
                .Flatten()
                .ToArray();
            return Encoding.Unicode.GetString(wideChars);
        }

        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            var chars = reader.AsByteStream()
                .TakeWhile(c => c != 0)
                .ToArray();
            return Encoding.UTF8.GetString(chars);
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
