using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Met.Core
{
    public enum PacketType : UInt32
    {
        Request = 0u,
        Response = 1u,
        PlainRequest = 10u,
        PlainResponse = 11u
    }

    public class Packet
    {
        private PacketType type;

        public Dictionary<TlvType, List<Tlv>> Tlvs { get; private set; }

        private Packet()
        {
            this.Tlvs = new Dictionary<TlvType, List<Tlv>>();
        }

        public Packet(byte[] data)
            : this()
        {
            var xorKey = new byte[4];
            Array.Copy(data, xorKey, 4);
            XorBytes(xorKey, ref data);
            ParseData(ref data);
        }

        public void Add(Tlv tlv)
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
    }
}
