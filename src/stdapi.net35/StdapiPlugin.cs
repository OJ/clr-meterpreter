using Met.Core;
using Met.Core.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            manager.RegisterFunction(this.Name, "core_channel_open", false, this.CoreChannelOpen);
        }

        public void Unregister(PluginManager manager)
        {
        }

        // TODO: move this to a channel manager of some kind
        private Packet CoreChannelOpen(Packet request)
        {
            return request.CreateResponse();
        }
    }
}
