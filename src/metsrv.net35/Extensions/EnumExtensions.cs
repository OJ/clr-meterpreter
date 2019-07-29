using Met.Core.Proto;
using System;

namespace Met.Core.Extensions
{
    public static class EnumExtensions
    {
        public static MetaType ToMetaType(this TlvType tlvType)
        {
            return (MetaType)((TlvType)MetaType.All & tlvType);
        }

        public static bool HasFlag(this Enum mask, Enum flag)
        {
            return (Convert.ToUInt64(mask) & Convert.ToUInt64(flag)) == Convert.ToUInt64(flag);
        }
    }
}
