using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Met.Core.Extensions;

namespace Met.Core.Proto
{
    public enum PacketResult : UInt32
    {
        Success = 0u,
        CallNotImplemented = 120u,
    }

    public enum PacketType : UInt32
    {
        Request = 0u,
        Response = 1u,
        PlainRequest = 10u,
        PlainResponse = 11u
    }

    public class Packet
    {
        private const int HEADER_SIZE = 4 + 16 + 4 + 4 + 4;
        private const int OFFSET_LENGTH = 24;

        private PacketType type;
        private RNGCryptoServiceProvider random = null;

        public Dictionary<TlvType, List<Tlv>> Tlvs { get; private set; }

        public string RequestId
        {
            get { return this.Tlvs[TlvType.RequestId].First().ValueAsString(); }
        }

        public string Method
        {
            get { return this.Tlvs[TlvType.Method].First().ValueAsString(); }
        }

        public PacketResult Result
        {
            get { return (PacketResult)this.Tlvs[TlvType.Result].First().ValueAsDword(); }
            set { this.Add(TlvType.Result, (UInt32)value); }
        }

        private RNGCryptoServiceProvider Random
        {
            get
            {
                if (this.random == null)
                {
                    this.random = new RNGCryptoServiceProvider();
                }

                return this.random;
            }
        }

        private Packet()
        {
            this.Tlvs = new Dictionary<TlvType, List<Tlv>>();
        }

        public Packet(byte[] data)
            : this()
        {
            ParseData(ref data);
        }

        public Packet(BinaryReader reader)
            : this()
        {
            var header = reader.ReadBytes(HEADER_SIZE);
            var clonedHeader = header.Clone() as byte[];
            var packetBody = default(byte[]);
            var xorKey = new byte[4];
            Array.Copy(clonedHeader, xorKey, xorKey.Length);
            XorBytes(xorKey, ref clonedHeader);

            using (var headerStream = new MemoryStream(clonedHeader))
            using (var headerReader = new BinaryReader(headerStream))
            {
                headerReader.BaseStream.Seek(OFFSET_LENGTH, SeekOrigin.Begin);
                var bytesToRead = headerReader.ReadDword() - 8;
                packetBody = reader.ReadBytes((int)bytesToRead);
            }

            var data = new byte[header.Length + packetBody.Length];
            Array.Copy(header, data, header.Length);
            Array.Copy(packetBody, 0, data, header.Length, packetBody.Length);

            ParseData(ref data);
        }

        public byte[] ToRaw(byte[] sessionGuid)
        {
            var packetData = default(byte[]);
            using (var packetStream = new MemoryStream())
            using (var writer = new BinaryWriter(packetStream))
            {
                var tlvData = default(byte[]);

                using (var tlvStream = new MemoryStream())
                using (var tlvWriter = new BinaryWriter(tlvStream))
                {
                    foreach (var tlv in this.Tlvs.Values.AsEnumerable().Flatten())
                    {
                        tlv.ToRaw(tlvWriter);
                    }
                    tlvData = tlvStream.ToArray();
                }

                // Write a zero XOR key
                writer.WriteDword(0u);

                // Write a blank session GUID
                writer.Write(sessionGuid);

                // Write ENC_NONE as the encryption flags
                writer.WriteDword(0u);

                // Specify the TLV data length
                writer.WriteDword((UInt32)tlvData.Length + 8u);

                // Specify the type
                writer.WritePacketType(this.type);

                writer.Write(tlvData);

                packetData = packetStream.ToArray();
            }

            var xorKey = GenerateXorKey();
            XorBytes(xorKey, ref packetData);

            return packetData;
        }

        public Packet CreateResponse()
        {
            var response = new Packet
            {
                type = this.type == PacketType.Request ? PacketType.Response : PacketType.PlainResponse,
            };

            response.Add(TlvType.RequestId, this.RequestId);
            response.Add(TlvType.Method, this.Method);

            return response;
        }

        public Tlv Add(TlvType type, PacketResult value)
        {
            return this.Add(new Tlv(type, (UInt32)value));
        }

        public Tlv Add(TlvType type, string value)
        {
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, bool value)
        {
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, byte[] value)
        {
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, UInt32 value)
        {
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, UInt64 value)
        {
            return this.Add(new Tlv(type, value));
        }

        public Tlv AddGroup(TlvType type)
        {
            return this.Add(new Tlv(type));
        }

        public Tlv Add(Tlv tlv)
        {
            var tlvs = default(List<Tlv>);

            if (this.Tlvs.TryGetValue(tlv.Type, out tlvs))
            {
                tlvs.Add(tlv);
            }
            else
            {
                this.Tlvs.Add(tlv.Type, new List<Tlv> { tlv });
            }

            return tlv;
        }

#if DEBUG
        public override string ToString()
        {
            var s = new StringBuilder();
            s.AppendFormat("Packet Type: {0}\n", this.type);
            foreach (var k in this.Tlvs)
            {
                foreach (var tlv in k.Value)
                {
                    s.AppendFormat(tlv.ToString());
                }
            }
            return s.ToString();
        }
#endif

        private void ParseData(ref byte[] data)
        {
            var xorKey = new byte[4];
            Array.Copy(data, xorKey, 4);
            XorBytes(xorKey, ref data);

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // Skip the first 28 bytes which contains:
                // XOR Key (4) session GUID (16) encryption flags (4) length (4)
                reader.BaseStream.Seek(28, SeekOrigin.Begin);
                this.type = reader.ReadPacketType();

                while (!reader.IsFinished())
                {
                    Add(new Tlv(reader));
                }
            }
        }

        private void XorBytes(byte[] xorKey, ref byte[] target)
        {
            for (int i = 0; i < target.Length; ++i)
            {
                target[i] ^= xorKey[i % xorKey.Length];
            }
        }

        private byte[] GenerateXorKey()
        {
            var bytes = new byte[4];
            this.Random.GetBytes(bytes);
            return bytes;
        }
    }
}
