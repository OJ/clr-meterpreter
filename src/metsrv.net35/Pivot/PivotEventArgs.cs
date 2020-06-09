using System;

namespace Met.Core.Pivot
{
    public class PivotEventArgs : EventArgs
    {
        public Pivot Pivot { get; }

        public PivotEventArgs(Pivot pivot)
        {
            Pivot = pivot;
        }
    }
}
