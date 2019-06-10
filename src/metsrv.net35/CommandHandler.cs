using Met.Core.Proto;
using System.Management;
using System.Linq;
using System;

namespace Met.Core
{
    public class CommandHandler
    {
        public void Register(PluginManager manager)
        {
            manager.RegisterFunction(string.Empty, "core_machine_id", false, this.CoreMachineId);
        }

        private InlineProcessingResult CoreMachineId(Packet request, Packet response)
        {
            var serialNumber = "1234-4324";
            var machineName = Environment.MachineName;
            var machineId = string.Format("{0}:{1}", serialNumber, machineName);

            response.Add(TlvType.MachineId, machineId);
            response.Result = PacketResult.Success;

            return InlineProcessingResult.Continue;
        }
    }
}
