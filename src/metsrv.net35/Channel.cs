namespace Met.Core
{
    public abstract class Channel
    {
        private static int channelIdentifier = 0;

        private object idLock = new object();
        private uint? channelId = null;

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
    }
}