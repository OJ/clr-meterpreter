using Met.Core;
using Met.Core.Proto;

namespace Met.Stdapi
{
    public class SysPower
    {
        public void Register(string extName, PluginManager manager)
        {
            manager.RegisterFunction(extName, "stdapi_sys_power_exitwindows", false, this.ExitWindows);
        }


        private InlineProcessingResult ExitWindows(Packet request, Packet response)
        {
#if WECARE
do {
		if(OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &token) == 0) {
			result = GetLastError();
			break;
		}

		if(LookupPrivilegeValue(NULL, SE_SHUTDOWN_NAME, &tkp.Privileges[0].Luid) == 0) {
			result = GetLastError();
			break;
		}

		tkp.PrivilegeCount = 1;
		tkp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

		if(AdjustTokenPrivileges(token, FALSE, &tkp, 0, NULL, NULL) == 0) {
			result = GetLastError();
			break;
		}

		if(ExitWindowsEx(flags, reason) == 0) {
			result = GetLastError();
			break;
		}
	} while(0);
#endif
            var flags = request.Tlvs[TlvType.StdapiPowerFlags][0].ValueAsDword();
            var reason = request.Tlvs[TlvType.StdapiPowerReason][0].ValueAsDword();

            var result = Core.Native.User32.ExitWindowsEx((Core.Native.User32.ShutdownFlags)flags, (Core.Native.User32.ShutdownReason)reason);

            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }
    }
}
