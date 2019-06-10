using Met.Core;
using Met.Core.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Met.Stdapi
{
    public class StdapiPlugin : IPlugin
    {
        public string Name => "stdapi";

        public void Init(byte[] initBytes)
        {
        }

        public void Register(PluginManager manager)
        {
            manager.RegisterFunction(this.Name, "stdapi_sys_config_getuid", false, this.SysConfigGetUid);
            manager.RegisterFunction(this.Name, "core_channel_open", false, this.CoreChannelOpen);
        }

        public void Unregister(PluginManager manager)
        {
        }

        private InlineProcessingResult SysConfigGetUid(Packet request, Packet response)
        {
            // TODO: validate that this works correctly when we impersonate other users or use `getsystem`
            response.Add(TlvType.StdapiUserName, WindowsIdentity.GetCurrent().Name);
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        // TODO: move this to a channel manager of some kind
        private InlineProcessingResult CoreChannelOpen(Packet request, Packet response)
        {
            return InlineProcessingResult.Continue;
        }
    }
}
