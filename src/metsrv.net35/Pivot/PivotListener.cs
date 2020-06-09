using System;

namespace Met.Core.Pivot
{
    public abstract class PivotListener
    {
        public Guid Id { get; }
        protected IPacketDispatcher PacketDispatcher { get; }

        public delegate void PivotAddedHandler(PivotEventArgs args);

        public event PivotAddedHandler PivotAdded; 

        public PivotListener(IPacketDispatcher packetDispatcher, Guid id)
        {
            PacketDispatcher = packetDispatcher;
            Id = id;
        }

        protected void FirePivotAdded(Pivot pivot)
        {
            PivotAdded?.Invoke(new PivotEventArgs(pivot));
        }
    }
}
