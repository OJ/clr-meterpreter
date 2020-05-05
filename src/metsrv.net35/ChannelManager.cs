using Met.Core.Proto;
using System;
using System.Collections.Generic;

namespace Met.Core
{
    public class ChannelManager
    {
        Dictionary<string, Func<Packet, Packet, Channel>> channelCreators = null;

        public ChannelManager()
        {
            this.channelCreators = new Dictionary<string, Func<Packet, Packet, Channel>>();
        }

        public void RegisterChannelCreator(string channelType, Func<Packet, Packet, Channel> handler)
        {
            this.channelCreators[channelType] = handler;
        }

        public void RegisterCommands(PluginManager pluginManager)
        {
            pluginManager.RegisterFunction(string.Empty, "core_channel_open", false, this.ChannelOpen);
        }

        private InlineProcessingResult ChannelOpen(Packet request, Packet response)
        {
            response.Result = PacketResult.CallNotImplemented;
            var channelType = request.Tlvs[TlvType.ChannelType][0].ValueAsString();
            Func<Packet, Packet, Channel> handler = null;

            if (this.channelCreators.TryGetValue(channelType, out handler))
            {
                var newChannel = handler(request, response);

                response.Add(TlvType.ChannelId, newChannel.ChannelId);

                // TODO: stash the new channel somewhere and manage it
                response.Result = PacketResult.Success;
            }
            return InlineProcessingResult.Continue;
        }
    }
}