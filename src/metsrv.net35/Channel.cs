using System;
using Met.Core.Extensions;
using Met.Core.Proto;

namespace Met.Core
{
    public abstract class Channel
    {
        private static int channelIdentifier = 0;

        private object idLock = new object();
        private uint? channelId = null;

        protected ChannelManager ChannelManager
        {
            get; private set;
        }

        public event EventHandler ChannelClosed;

        public virtual void Interact(bool interact)
        {
        }

        public uint ChannelId
        {
            get
            {
                if (!this.channelId.HasValue)
                {
                    lock (this.idLock)
                    {
                        if (!this.channelId.HasValue)
                        {
                            this.channelId = (uint)System.Threading.Interlocked.Increment(ref channelIdentifier);
                        }
                    }
                }

                return this.channelId.Value;
            }
            protected set
            {
                this.channelId = value;
            }
        }

        protected Channel(ChannelManager channelManager)
        {
            this.ChannelManager = channelManager;
        }

        protected void FireClosedEvent()
        {
            ChannelClosed?.Invoke(this, new EventArgs());
        }

        public virtual PacketResult IsEof(Packet request, Packet response)
        {
            return PacketResult.CallNotImplemented;
        }

        public virtual PacketResult Tell(Packet request, Packet response)
        {
            return PacketResult.CallNotImplemented;
        }

        public virtual PacketResult Write(Packet request, Packet response)
        {
            var data = request.Tlvs.TryGetTlvValueAsRaw(TlvType.ChannelData);
            var bytesToWrite = (int)request.Tlvs.TryGetTlvValueAsDword(TlvType.Length);
            var bytesWritten = default(int);
            var result = this.WriteInternal(data, bytesToWrite, out bytesWritten);

            if (result == PacketResult.Success)
            {
                response.Add(TlvType.Length, bytesWritten);
            }

            return result;
        }

        protected virtual PacketResult WriteInternal(byte[] data, int bytesToWrite, out int bytesWritten)
        {
            bytesWritten = 0;
            return PacketResult.CallNotImplemented;
        }

        public virtual PacketResult Read(Packet request, Packet response)
        {
            var bytesToRead = request.Tlvs.TryGetTlvValueAsDword(TlvType.Length);
            var buffer = new byte[bytesToRead];
            var bytesRead = default(int);
            var result = this.ReadInternal(buffer, out bytesRead);

            if (result == PacketResult.Success)
            {
                // TODO: handle channel flags/etc
                response.Add(TlvType.ChannelData, buffer, bytesRead);
                response.Add(TlvType.Length, bytesRead);
            }

            return result;
        }

        protected virtual PacketResult ReadInternal(byte[] buffer, out int bytesRead)
        {
            bytesRead = 0;
            return PacketResult.CallNotImplemented;
        }

        public abstract void Close();
    }
}