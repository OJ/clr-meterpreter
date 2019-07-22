using Met.Core;
using Met.Core.Extensions;
using Met.Core.Proto;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Met.Stdapi
{
    public class SysProcess
    {
        public void Register(string extName, PluginManager manager)
        {
            manager.RegisterFunction(extName, "stdapi_sys_process_getpid", false, this.GetPid);
            manager.RegisterFunction(extName, "stdapi_sys_process_kill", false, this.Kill);
            manager.RegisterFunction(extName, "stdapi_sys_process_get_processes", false, this.GetProcesses);
        }

        private InlineProcessingResult Kill(Packet request, Packet response)
        {
            var pidList = default(List<Tlv>);

            if (request.Tlvs.TryGetValue(TlvType.StdapiProcessId, out pidList))
            {
                foreach (var pid in pidList.Select(tlv => tlv.ValueAsDword()))
                {
                    var process = Process.GetProcessById((int)pid);
                    if (process != null)
                    {
                        process.Kill();
                    }
                }
            }

            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult GetPid(Packet request, Packet response)
        {
            response.Add(TlvType.StdapiProcessId, (uint)Process.GetCurrentProcess().Id);
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult GetProcesses(Packet request, Packet response)
        {
            foreach (var process in Process.GetProcesses())
            {
                var processTlv = response.AddGroup(TlvType.StdapiProcessGroup);

                processTlv.Add(TlvType.StdapiProcessId, (uint)process.Id);
                processTlv.Add(TlvType.StdapiProcessName, process.ProcessName);

                var fileName = process.Pokemon(p => p.MainModule.FileName);
                if (fileName != null)
                {
                    processTlv.Add(TlvType.StdapiProcessPath, fileName);
                }

                var userName = process.GetUserName();
                if (userName != null)
                {
                    processTlv.Add(TlvType.StdapiUserName, userName);
                }
                processTlv.Add(TlvType.StdapiProcessSession, (uint)process.SessionId);

                var parent = process.GetParentProcess();
                if (parent != null)
                {
                    processTlv.Add(TlvType.StdapiProcessParentProcessId, (uint)parent.Id);
                }

                process.Pokemon(p => p.IsWow64(), r => processTlv.Add(TlvType.StdapiProcessArch, (uint)(r ? SystemArchictecture.X86 : SystemArchictecture.X64)));
            }

            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

    }
}
