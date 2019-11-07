using Met.Core.Proto;
using System.Collections.Generic;

namespace Met.Core.Extensions
{
    public static class TlvDictionaryExtensions
    {
        public static Tlv TryGetTlv(this Dictionary<TlvType, List<Tlv>> dict, TlvType type)
        {
            var list = default(List<Tlv>);
            if (dict.TryGetValue(type, out list) && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public static byte[] TryGetTlvValueAsRaw(this Dictionary<TlvType, List<Tlv>> dict, TlvType type, byte[] def = null)
        {
            var list = default(List<Tlv>);
            if (dict.TryGetValue(type, out list) && list.Count > 0)
            {
                return list[0].ValueAsRaw();
            }

            return def;
        }

        public static uint TryGetTlvValueAsDword(this Dictionary<TlvType, List<Tlv>> dict, TlvType type, uint def = 0)
        {
            var list = default(List<Tlv>);
            if (dict.TryGetValue(type, out list) && list.Count > 0)
            {
                return list[0].ValueAsDword();
            }

            return def;
        }

        public static bool TryGetTlvValueAsBool(this Dictionary<TlvType, List<Tlv>> dict, TlvType type, bool def = false)
        {
            var list = default(List<Tlv>);
            if (dict.TryGetValue(type, out list) && list.Count > 0)
            {
                return list[0].ValueAsBool();
            }

            return def;
        }

        public static ulong TryGetTlvValueAsQword(this Dictionary<TlvType, List<Tlv>> dict, TlvType type, ulong def = 0)
        {
            var list = default(List<Tlv>);
            if (dict.TryGetValue(type, out list) && list.Count > 0)
            {
                return list[0].ValueAsQword();
            }

            return def;
        }

        public static string TryGetTlvValueAsString(this Dictionary<TlvType, List<Tlv>> dict, TlvType type, string def = "")
        {
            var list = default(List<Tlv>);
            if (dict.TryGetValue(type, out list) && list.Count > 0)
            {
                return list[0].ValueAsString();
            }

            return def;
        }
    }
}
