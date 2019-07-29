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
    }
}
