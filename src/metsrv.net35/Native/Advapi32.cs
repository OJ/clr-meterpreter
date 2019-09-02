using System;
using System.Runtime.InteropServices;

namespace Met.Core.Native
{
    public static class Advapi32
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
    }
}
