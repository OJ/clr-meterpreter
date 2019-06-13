using Met.Core;
using Met.Core.Proto;
using System.Net.NetworkInformation;

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
                var ip = ni.GetIPProperties();
                foreach (var addr in ip.UnicastAddresses)
                {
                }
            }

            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }
    }
}
