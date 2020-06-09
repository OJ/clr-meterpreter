using System;

namespace Met.Core.Pivot
{
    public abstract class Pivot
    {
        public Guid Id { get; }
        protected IPacketDispatcher PacketDispacher { get; }

        public Pivot(IPacketDispatcher packetDispacher)
        {
            Id = Guid.NewGuid();
            PacketDispacher = packetDispacher;
        }
    }
}
