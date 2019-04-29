using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Met.Core
{
    [Flags]
    enum TlvType : UInt32
    {
        MetaNone       = 0u,
        MetaString     = (1u << 16),
        MetaUint       = (1u << 17),
        MetaRaw        = (1u << 18),
        MetaBool       = (1u << 19),
        MetaQword      = (1u << 20),
        MetaCompressed = (1u << 29),
        MetaGroup      = (1u << 30),
        MetaComplex    = (1u << 31),

        Reserved       = 0u,
        Extensions     = 20000u,
        User           = 40000u,
        Temp           = 60000u,

        Result         = MetaUint | 4u
    }

    class PacketParser
    {
        public Dictionary<TlvType, List<Tlv>> Parse(Packet p)
        {
            var result = new Dictionary<TlvType, List<Tlv>>();
            var data = new byte[1]; // get this from the packet

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                while (reader.PeekChar() != -1)
                {
                    var tlv = new Tlv(reader);

                    if (!result.ContainsKey(tlv.Type))
                    {
                        result[tlv.Type] = new List<Tlv>();
                    }

                    result[tlv.Type].Add(tlv);
                }
            }

            return result;
        }
    }

    class Tlv
    {
        private object value;

        public TlvType Type { get; private set; }

        public Tlv(BinaryReader reader)
        {
            var length = reader.ReadUInt32() - 8;
            Type = (TlvType)reader.ReadUInt32();

            if ((Type & TlvType.MetaUint) == TlvType.MetaUint && length == sizeof(UInt32))
            {
                value = reader.ReadInt32();
            }
            else if ((Type & TlvType.MetaRaw) == TlvType.MetaRaw)
            {
                value = reader.ReadBytes((int)length);
            }
            else if ((Type & TlvType.MetaString) == TlvType.MetaString)
            {
                var bytes = reader.ReadBytes((int)length);
                value = Encoding.UTF8.GetString(bytes);
            }
        }

        public T GetValue<T>(T defaultValue = default(T))
        {
            if (typeof(T) == value.GetType())
            {
                return (T)value;
            }
            return defaultValue;
        }

        public void Foo()
        {
            var name = GetValue("OJ");
            var id = GetValue(0u);
        }
    }
}
