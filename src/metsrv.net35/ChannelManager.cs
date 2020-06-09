using Met.Core.Extensions;
using Met.Core.Proto;
using System;
using System.Collections.Generic;

namespace Met.Core
{
    public class ChannelManager
    {
        private Dictionary<string, Func<ChannelManager, Packet, Packet, Channel>> channelCreators = null;
        private Dictionary<uint, Channel> activeChannels = null;

        private IPacketDispatcher packetDispatcher;

        public ChannelManager(IPacketDispatcher packetDispatcher)
        {
            this.channelCreators = new Dictionary<string, Func<ChannelManager, Packet, Packet, Channel>>();
            this.activeChannels = new Dictionary<uint, Channel>();
            this.packetDispatcher = packetDispatcher;
        }

        public void RegisterChannelCreator(string channelType, Func<ChannelManager, Packet, Packet, Channel> handler)
        {
            this.channelCreators[channelType] = handler;
        }

        public void RegisterCommands(PluginManager pluginManager)
        {
            pluginManager.RegisterFunction(string.Empty, "core_channel_open", false, this.ChannelOpen);
            pluginManager.RegisterFunction(string.Empty, "core_channel_write", false, this.ChannelWrite);
            pluginManager.RegisterFunction(string.Empty, "core_channel_close", false, this.ChannelClose);
            pluginManager.RegisterFunction(string.Empty, "core_channel_interact", false, this.ChannelInteract);
            pluginManager.RegisterFunction(string.Empty, "core_channel_read", false, this.ChannelRead);
            pluginManager.RegisterFunction(string.Empty, "core_channel_tell", false, this.ChannelTell);
            pluginManager.RegisterFunction(string.Empty, "core_channel_eof", false, this.ChannelIsEof);
        }

        public void Manage(Channel channel)
        {
            this.activeChannels[channel.ChannelId] = channel;
            channel.ChannelClosed += ChannelClosed;
        }

        public void Dispatch(Packet packet)
        {
            this.packetDispatcher.DispatchPacket(packet);
        }

        private InlineProcessingResult ChannelAct(Packet request, Packet response, Func<Channel, PacketResult> action)
        {
            var channelId = request.Tlvs.TryGetTlvValueAsDword(TlvType.ChannelId);
            var interact = request.Tlvs.TryGetTlvValueAsBool(TlvType.Bool);
            response.Result = PacketResult.CallNotImplemented;

            var channel = default(Channel);
            if (this.activeChannels.TryGetValue(channelId, out channel))
            {
                response.Result = action(channel);
            }

            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult ChannelIsEof(Packet request, Packet response)
        {
            return ChannelAct(request, response, channel =>
            {
                response.Add(TlvType.ChannelId, channel.ChannelId);
                return channel.IsEof(request, response);
            });
        }

        private InlineProcessingResult ChannelTell(Packet request, Packet response)
        {
            return ChannelAct(request, response, channel =>
            {
                response.Add(TlvType.ChannelId, channel.ChannelId);
                return channel.Tell(request, response);
            });
        }

        private InlineProcessingResult ChannelRead(Packet request, Packet response)
        {
            return ChannelAct(request, response, channel =>
            {
                response.Add(TlvType.ChannelId, channel.ChannelId);
                return channel.Read(request, response);
            });
        }

        private InlineProcessingResult ChannelInteract(Packet request, Packet response)
        {
            return ChannelAct(request, response, channel =>
            {
                channel.Interact(request.Tlvs.TryGetTlvValueAsBool(TlvType.Bool));
                return PacketResult.Success;
            });
        }

        private InlineProcessingResult ChannelClose(Packet request, Packet response)
        {
            return ChannelAct(request, response, channel =>
            {
                channel.Close();
                this.activeChannels.Remove(channel.ChannelId);
                return PacketResult.Success;
            });
        }

        private InlineProcessingResult ChannelWrite(Packet request, Packet response)
        {
            return ChannelAct(request, response, channel =>
            {
                response.Add(TlvType.ChannelId, channel.ChannelId);
                return channel.Write(request, response);
            });
        }

        private InlineProcessingResult ChannelOpen(Packet request, Packet response)
        {
            response.Result = PacketResult.CallNotImplemented;
            var channelType = request.Tlvs.TryGetTlvValueAsString(TlvType.ChannelType);
            Func<ChannelManager, Packet, Packet, Channel> handler = null;

            if (this.channelCreators.TryGetValue(channelType, out handler))
            {
                var newChannel = handler(this, request, response);

                if (newChannel != null)
                {
                    this.Manage(newChannel);
                    response.Add(TlvType.ChannelId, newChannel.ChannelId);
                    response.Result = PacketResult.Success;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(string.Format("[core_channel_open] unable to create type {0}", channelType));
            }
            return InlineProcessingResult.Continue;
        }

        private void ChannelClosed(object sender, EventArgs e)
        {
            var channel = (Channel)sender;
            var packet = new Packet("core_channel_close");
            packet.Add(TlvType.ChannelId, channel.ChannelId);
            this.Dispatch(packet);
            this.activeChannels.Remove(channel.ChannelId);
        }
    }
}