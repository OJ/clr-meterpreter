using Met.Core.Extensions;
using Met.Core.Pivot;
using Met.Core.Proto;
using System;
using System.Collections.Generic;

namespace Met.Core
{
    public class PivotManager
    {
        private readonly IPacketDispatcher packetDispatcher;
        private readonly Dictionary<Guid, PivotListener> pivotListeners;
        private readonly Dictionary<Guid, Pivot.Pivot> pivots;

        public PivotManager(IPacketDispatcher packetDispatcher)
        {
            this.packetDispatcher = packetDispatcher;
            this.pivotListeners = new Dictionary<Guid, PivotListener>();
            this.pivots = new Dictionary<Guid, Pivot.Pivot>();
        }

        public void RegisterCommands(PluginManager pluginManager)
        {
            pluginManager.RegisterFunction(string.Empty, "core_pivot_add", false, this.PivotAdd);
        }

        private InlineProcessingResult PivotAdd(Packet request, Packet response)
        {
            // TODO: when we support more pivot types, don't assume this is a named pipe
            var pipeName = request.Tlvs.TryGetTlvValueAsString(TlvType.PivotNamedPipeName);
            var stageData = request.Tlvs.TryGetTlvValueAsRaw(TlvType.PivotStageData);
            var pivotId = new Guid(request.Tlvs.TryGetTlvValueAsRaw(TlvType.PivotId));
            var listener = new NamedPipePivotListener(this.packetDispatcher, pivotId, pipeName, stageData);

            listener.PivotAdded += PivotAdded;

            this.pivotListeners.Add(listener.Id, listener);

            response.Result = PacketResult.Success;

            return InlineProcessingResult.Continue;
        }

        private void PivotAdded(PivotEventArgs args)
        {
            this.pivots[args.Pivot.Id] = args.Pivot;
        }
    }
}
