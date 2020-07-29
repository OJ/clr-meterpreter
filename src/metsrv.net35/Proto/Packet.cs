using Met.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Met.Core.Proto
{
    public enum PacketResult : UInt32
    {
        Success = 0u,
        InvalidFunction = 1u,
        InvalidData = 13u,
        CallNotImplemented = 120u,
        BadArguments = 160u,
        ErrorAlreadyExists = 183u,
    }

    public enum PacketType : UInt32
    {
        Request = 0u,
        Response = 1u,
        PlainRequest = 10u,
        PlainResponse = 11u
    }

    public class Packet : ITlv
    {
        public const int HEADER_SIZE = 4 + 16 + 4 + 4 + 4;
        private const int ENC_LENGTH = 20;
        private const int OFFSET_LENGTH = 24;

        private PacketType type;
        private RNGCryptoServiceProvider random = null;

        public Dictionary<TlvType, List<Tlv>> Tlvs { get; private set; }

        public string RequestId
        {
            get { return this.Tlvs[TlvType.RequestId].First().ValueAsString(); }
            private set
            {
                this.Tlvs.Remove(TlvType.RequestId);
                this.Add(TlvType.RequestId, value);
            }
        }

        public string Method
        {
            get { return this.Tlvs[TlvType.Method].First().ValueAsString(); }
            private set
            {
                this.Tlvs.Remove(TlvType.Method);
                this.Add(TlvType.Method, value);
            }
        }

        public PacketResult Result
        {
            get { return (PacketResult)this.Tlvs[TlvType.Result].First().ValueAsDword(); }
            set
            {
                this.Tlvs.Remove(TlvType.Result);
                this.Add(TlvType.Result, (UInt32)value);
            }
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

        public Packet(string method)
            : this()
        {
            this.type = PacketType.Request;
            this.Method = method;
            this.RequestId = Guid.NewGuid().ToString().Replace("-", "");
        }

        public Packet(BinaryReader reader, PacketEncryptor packetEncryptor = null)
            : this()
        {
            packetEncryptor = packetEncryptor ?? PacketEncryptor.Blank;
            var header = reader.ReadBytes(HEADER_SIZE);
            var packetBody = default(byte[]);
            var xorKey = new byte[4];
            Array.Copy(header, xorKey, xorKey.Length);
            header.Xor(xorKey);

            var encrypted = false;
            var packetType = PacketType.Request;

            using (var headerStream = new MemoryStream(header))
            using (var headerReader = new BinaryReader(headerStream))
            {
                // Move to the encryption flags
                headerReader.BaseStream.Seek(ENC_LENGTH, SeekOrigin.Begin);
                var encFlags = headerReader.ReadDword();
                var bytesToRead = headerReader.ReadDword() - 8;
                packetType = headerReader.ReadPacketType();
                packetBody = reader.ReadBytes((int)bytesToRead);

                encrypted = encFlags == PacketEncryptor.ENC_AES256;
            }

            packetBody.Xor(xorKey);
            if (encrypted)
            {
                // TODO: if we don't have a packet encryptor, then we should probably
                // bail out.
                packetBody = packetEncryptor.AesDecrypt(packetBody);
            }

            ParseData(packetType, ref packetBody);
        }

        public byte[] ToRaw(Guid sessionGuid, PacketEncryptor packetEncryptor = null)
        {
            return this.ToRaw(sessionGuid.ToByteArray(), packetEncryptor);
        }

        public byte[] ToRaw(byte[] sessionGuid, PacketEncryptor packetEncryptor = null)
        {
            var packetData = default(byte[]);
            packetEncryptor = packetEncryptor ?? PacketEncryptor.Blank;

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
                    tlvData = packetEncryptor.Encrypt(tlvStream.ToArray());
                }

                // Write a zero XOR key, which gets filled in later.
                writer.WriteDword(0u);

                writer.Write(sessionGuid);
                writer.WriteDword(packetEncryptor.Flags);
                writer.WriteDword((UInt32)tlvData.Length + 8u);
                writer.WritePacketType(this.type);
                writer.Write(tlvData);

                packetData = packetStream.ToArray();
            }

            packetData.Xor(GenerateXorKey());

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

        public Tlv Add(TlvType type, byte[] value, int size)
        {
            return this.Add(new Tlv(type, value, size));
        }

        public Tlv Add(TlvType type, Int32 value)
        {
            return this.Add(new Tlv(type, (UInt32)value));
        }

        public Tlv Add(TlvType type, UInt32 value)
        {
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, UInt64 value)
        {
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, Int64 value)
        {
            return this.Add(new Tlv(type, (UInt64)value));
        }

        public Tlv Add<T>(TlvType type, T value) where T : struct
        {
            var meta = type.ToMetaType();
            if (meta != MetaType.Raw && meta != MetaType.Complex)
            {
                throw new ArgumentException(string.Format("Unable to serialise struct to type: {0}", meta));
            }

            return this.Add(new Tlv(type, value.ToByteArray()));
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

        private void ParseData(PacketType type, ref byte[] data)
        {
            this.type = type;

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                while (!reader.IsFinished())
                {
                    Add(new Tlv(reader));
                }
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
