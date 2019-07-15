using Met.Core.Proto;

namespace Met.Core.Extensions
{
    public static class EnumExtensions
    {
        public static MetaType ToMetaType(this TlvType tlvType)
        {
            return (MetaType)((TlvType)MetaType.All & tlvType);
        }

        public static bool HasFlag(this MetaType metaType, MetaType flag)
        {
            return (metaType & flag) == flag;
        }
    }
}
