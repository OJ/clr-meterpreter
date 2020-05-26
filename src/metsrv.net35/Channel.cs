using System;
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

        public abstract PacketResult Write(Packet request, Packet response, out int bytesWritten);
        public abstract void Close();
    }
}