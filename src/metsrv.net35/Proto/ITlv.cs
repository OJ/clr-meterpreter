using System;
using System.Collections.Generic;

namespace Met.Core.Proto
{
    public interface ITlv
    {
        Dictionary<TlvType, List<Tlv>> Tlvs { get; }

        Tlv Add(TlvType type, string value);
        Tlv Add(TlvType type, bool value);
        Tlv Add(TlvType type, byte[] value);
        Tlv Add(TlvType type, byte[] value, int size);
        Tlv Add(TlvType type, UInt32 value);
        Tlv Add(TlvType type, UInt64 value);
        Tlv Add<T>(TlvType type, T value) where T : struct;
        Tlv AddGroup(TlvType type);
        Tlv Add(Tlv tlv);
    }
}
