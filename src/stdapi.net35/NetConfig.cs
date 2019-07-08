using Met.Core;
using Met.Core.Extensions;
using Met.Core.Proto;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Met.Stdapi
{
    public class NetConfig
    {
        public void Register(string extName, PluginManager manager)
        {
            manager.RegisterFunction(extName, "stdapi_net_config_get_interfaces", false, this.GetInterfaces);
        }

        private InlineProcessingResult GetInterfaces(Packet request, Packet response)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var tlv = response.AddGroup(TlvType.StdapiNetworkInterface);
                tlv.Add(TlvType.StdapiMacName, ni.Description);

                var physical = ni.GetPhysicalAddress();
                tlv.Add(TlvType.StdapiMacAddr, physical.GetAddressBytes());

                var ip = ni.GetIPProperties();
                var ip4Props = ip.GetIPv4Properties();
                if (ip4Props != null)
                {
                    tlv.Add(TlvType.StdapiInterfaceIndex, ip4Props.Index);
                    tlv.Add(TlvType.StdapiInterfaceMtu, ip4Props.Mtu);
                }

                foreach (var addr in ip.UnicastAddresses.OrderBy(a => (uint)a.Address.AddressFamily))
                {
                    if (addr.Address.AddressFamily != AddressFamily.InterNetwork &&
                        addr.Address.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        continue;
                    }

                    tlv.Add(TlvType.StdapiIpPrefix, addr.GetPrefixLength());
                    tlv.Add(TlvType.StdapiIp, addr.Address.GetAddressBytes());

                    if (addr.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        var sid = (uint)addr.Address.ScopeId;

                        var b = new byte[]
                        {
                            (byte)(sid & 0xFF),
                            (byte)(sid >> 8 & 0xFF),
                            (byte)(sid >> 16 & 0xFF),
                            (byte)(sid >> 24 & 0xFF),
                        };
                        tlv.Add(TlvType.StdapiIp6Scope, b);
                    }
                    //else
                    //{
                    //    if (addr.IPv4Mask != null)
                    //    {
                    //        tlv.Add(TlvType.StdapiNetmask, addr.IPv4Mask.GetAddressBytes());
                    //    }
                    //}
                }
            }

            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }
    }
}
