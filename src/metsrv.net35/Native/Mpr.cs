using System;
using System.Runtime.InteropServices;

namespace Met.Core.Native
{
    public static class Mpr
    {
        public enum InfoLevel : int
        {
            UNIVERSAL_NAME_INFO_LEVEL = 0x00000001
        }

        public enum GetUniversalNameResult : int
        {
            ERROR_MORE_DATA = 234,
            NOERROR = 0
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern GetUniversalNameResult WNetGetUniversalName(
            string lpLocalPath,
            [MarshalAs(UnmanagedType.U4)]
            InfoLevel dwInfoLevel,
            IntPtr lpBuffer,
            [MarshalAs(UnmanagedType.U4)]
            ref int lpBufferSize);
    }
}
