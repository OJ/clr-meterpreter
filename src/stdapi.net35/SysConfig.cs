#define USERTLGETVERSION

using Met.Core;
using Met.Core.Proto;
using System.Security.Principal;
using System;
using Microsoft.Win32;
using System.Threading;
using System.Runtime.InteropServices;

namespace Met.Stdapi
{
    public class SysConfig
    {
        private const uint VER_NT_WORKSTATION = 1u;

        public void Register(string extName, PluginManager manager)
        {
            manager.RegisterFunction(extName, "stdapi_sys_config_getuid", false, this.GetUid);
            manager.RegisterFunction(extName, "stdapi_sys_config_sysinfo", false, this.GetSysinfo);
        }

        private InlineProcessingResult GetSysinfo(Packet request, Packet response)
        {
            response.Add(TlvType.StdapiComputerName, GetSystemMachineName());
            response.Add(TlvType.StdapiOperatingSystemName, GetOsVersionString());
            response.Add(TlvType.StdapiArchitecture, GetSystemArchitecture());
            response.Add(TlvType.StdapiLangSystem, GetSystemLocale());

            var domain = GetSystemDomainName();
            if (!string.IsNullOrEmpty(domain))
            {
                response.Add(TlvType.StdapiDomain, domain);
            }
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult GetUid(Packet request, Packet response)
        {
            // TODO: validate that this works correctly when we impersonate other users or use `getsystem`
            response.Add(TlvType.StdapiUserName, WindowsIdentity.GetCurrent().Name);
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private string GetSystemMachineName()
        {
            return Environment.MachineName;
        }

        private string GetSystemLocale()
        {
            Thread.CurrentThread.CurrentCulture.ClearCachedData();
            return Thread.CurrentThread.CurrentCulture.Name.Replace('-', '_');
        }

        private string GetSystemDomainName()
        {
            try
            {
                // TODO: run this on a domain-joined machine
                var currentDomain = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain();
                return currentDomain.Name;
            }
            catch
            {
                return null;
            }
        }

        private string GetSystemArchitecture()
        {
            var arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTUREW6432");
            if (string.IsNullOrEmpty(arch))
            {
                arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            }

            switch (arch)
            {
                case "AMD64": return "x64";
                default: return arch;
            }
        }

        [DllImport("ntdll.dll")]
        internal static extern int RtlGetVersion(ref RtlOSVersionInfoEx lpVersionInformation);

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
        internal struct RtlOSVersionInfoEx
        {
            internal uint dwOSVersionInfoSize;
            internal uint dwMajorVersion;
            internal uint dwMinorVersion;
            internal uint dwBuildNumber;
            internal uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string szCSDVersion;
            internal ushort wServicePackMajor;
            internal ushort wServicePackMinor;
            internal ushort wSuiteMask;
            internal byte wProductType;
            internal byte wReserved;
        }

        private string GetOsVersionString()
        {
            var name = "unknown";

            var v = new RtlOSVersionInfoEx
            {
                dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(RtlOSVersionInfoEx))
            };

            RtlGetVersion(ref v);
            var isWorkstation = v.wProductType == VER_NT_WORKSTATION;

            if (v.dwMajorVersion == 3)
            {
                name = "Windows NT 3.51";
            }
            else if (v.dwMajorVersion == 4)
            {
                if (v.dwPlatformId == (uint)PlatformID.Win32Windows)
                {
                    if (v.dwMinorVersion == 0)
                    {
                        name = "Windows 95";
                    }
                    else if (v.dwMinorVersion == 10)
                    {
                        name = "Windows 98";
                    }
                    else if (v.dwMinorVersion == 90)
                    {

                        name = "Windows ME";
                    }
                }
                else if (v.dwPlatformId == (uint)PlatformID.Win32NT)
                {
                    name = "Windows NT 4.0";
                }
            }
            else if (v.dwMajorVersion == 5)
            {
                if (v.dwMinorVersion == 0)
                {
                    name = "Windows 2000";
                }
                else if (v.dwMinorVersion == 1)
                {
                    name = "Windows XP";
                }
                else if (v.dwMinorVersion == 2)
                {
                    name = "Windows .NET Server";
                }
            }
            else if (v.dwMajorVersion == 6)
            {
                if (v.dwMinorVersion == 0)
                {
                    name = isWorkstation ? "Windows Vista" : "Windows 2008";
                }
                else if (v.dwMinorVersion == 1)
                {
                    name = isWorkstation ? "Windows 7" : "Windows 2008 R2";
                }
                else if (v.dwMinorVersion == 2)
                {
                    name = isWorkstation ? "Windows 8" : "Windows 2012";
                }
                else if (v.dwMinorVersion == 3)
                {
                    name = isWorkstation ? "Windows 8.1" : "Windows 2012 R2";
                }
            }
            else if (v.dwMajorVersion == 10)
            {
                name = isWorkstation ? "Windows 10" : "Windows 2016";
            }

            var os = default(string);
            if (string.IsNullOrEmpty(v.szCSDVersion))
            {
                os = string.Format("{0} (Build {1})", name, v.dwBuildNumber);
            }
            else
            {
                os = string.Format("{0} (Build {1}, {2})", name, v.dwBuildNumber, v.szCSDVersion);
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("OS String: {0}", os));
#endif
            return os;
        }
    }
}
