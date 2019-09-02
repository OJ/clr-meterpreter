using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Met.Core.Extensions
{
    public static class ProcessExtensions
    {
        public static Process GetParentProcess(this Process process)
        {
            try
            {
                return Core.Native.Ntdll.ParentProcessUtilities.GetParentProcess(process.Handle);
            }
            catch
            {
                return null;
            }
        }

        public static bool IsWow64(this Process process)
        {
            var result = false;

            Core.Native.Kernel32.IsWow64Process(process.Handle, out result);

            return result;
        }

        public static string GetUserName(this Process process)
        {
            IntPtr processHandle = IntPtr.Zero;
            try
            {
                Core.Native.Advapi32.OpenProcessToken(process.Handle, 8, out processHandle);
                using (var wi = new WindowsIdentity(processHandle))
                {
                    return wi.Name;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    Core.Native.Kernel32.CloseHandle(processHandle);
                }
            }
        }

    }
}
