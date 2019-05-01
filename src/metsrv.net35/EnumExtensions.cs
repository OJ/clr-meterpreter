namespace Met.Core
{
    public static class EnumExtensions
    {
        public static MetaType ToMetaType(this TlvType tlvType)
        {
            return (MetaType)((TlvType)MetaType.All & tlvType);
        }
    }
}
