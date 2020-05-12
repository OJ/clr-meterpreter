using Met.Core.Proto;
using System;
using System.Collections.Generic;

namespace Met.Core
{
    public class ChannelManager
    {
        private Dictionary<string, Func<Action<Packet>, Packet, Packet, Channel>> channelCreators = null;
        private Dictionary<uint, Channel> activeChannels = null;
        private readonly Action<Packet> packetDispatcher = null;

        public ChannelManager(Action<Packet> packetDispatcher)
        {
            this.channelCreators = new Dictionary<string, Func<Action<Packet>, Packet, Packet, Channel>>();
            this.activeChannels = new Dictionary<uint, Channel>();
            this.packetDispatcher = packetDispatcher;
        }

        public void RegisterChannelCreator(string channelType, Func<Action<Packet>, Packet, Packet, Channel> handler)
        {
            this.channelCreators[channelType] = handler;
        }

        public void RegisterCommands(PluginManager pluginManager)
        {
            pluginManager.RegisterFunction(string.Empty, "core_channel_open", false, this.ChannelOpen);
            pluginManager.RegisterFunction(string.Empty, "core_channel_write", false, this.ChannelWrite);
            pluginManager.RegisterFunction(string.Empty, "core_channel_close", false, this.ChannelClose);
        }

        private InlineProcessingResult ChannelClose(Packet request, Packet response)
        {
            var channelId = request.Tlvs[TlvType.ChannelId][0].ValueAsDword();
            response.Result = PacketResult.CallNotImplemented;

            var channel = default(Channel);
            if (this.activeChannels.TryGetValue(channelId, out channel))
            {
                channel.Close();
                this.activeChannels.Remove(channelId);
                response.Result = PacketResult.Success;
            }

            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult ChannelWrite(Packet request, Packet response)
        {
            var channelId = request.Tlvs[TlvType.ChannelId][0].ValueAsDword();
            response.Result = PacketResult.CallNotImplemented;

            var channel = default(Channel);
            if (this.activeChannels.TryGetValue(channelId, out channel))
            {
                response.Result = channel.Write(request, response);
            }

            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult ChannelOpen(Packet request, Packet response)
        {
            response.Result = PacketResult.CallNotImplemented;
            var channelType = request.Tlvs[TlvType.ChannelType][0].ValueAsString();
            Func<Action<Packet>, Packet, Packet, Channel> handler = null;

            if (this.channelCreators.TryGetValue(channelType, out handler))
            {
                var newChannel = handler(this.packetDispatcher, request, response);

                if (newChannel != null)
                {
                    this.activeChannels[newChannel.ChannelId] = newChannel;
                    response.Add(TlvType.ChannelId, newChannel.ChannelId);

                    newChannel.ChannelClosed += ChannelClosed;

                    response.Result = PacketResult.Success;
                }
            }
            return InlineProcessingResult.Continue;
        }

        private void ChannelClosed(object sender, EventArgs e)
        {
            var channel = (Channel)sender;
            var packet = new Packet("core_channel_close");
            packet.Add(TlvType.ChannelId, channel.ChannelId);
            this.packetDispatcher(packet);
            this.activeChannels.Remove(channel.ChannelId);
        }
    }
}