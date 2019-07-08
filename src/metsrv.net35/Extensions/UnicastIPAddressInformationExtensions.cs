using System.Net.NetworkInformation;

namespace Met.Core.Extensions
{
    public static class UnicastIPAddressInformationExtensions
    {
        public static uint GetPrefixLength(this UnicastIPAddressInformation addr)
        {
#if NET40
            return (uint)addr.GetPrivateProperty<int>("PrefixLength");
#else
            var adapterAddress = addr.GetPrivateField<object>("adapterAddress");
            return adapterAddress.GetPrivateField<uint>("length");
#endif
        }
    }
}
