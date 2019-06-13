using Met.Core;
using Met.Core.Proto;
using System.Security.Principal;
using System;
using Microsoft.Win32;
using System.Threading;

namespace Met.Stdapi
{
    public class SysConfig
    {
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

        private bool IsWorkstation()
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var value = key.GetValue("InstallationType");
            return value.ToString().ToLowerInvariant() == "client";
        }

        private string GetOsVersionString()
        {
            var name = "unknown";
            var isWorkstation = IsWorkstation();
            var v = Environment.OSVersion;

            if (v.Version.Major == 3)
            {
                name = "Windows NT 3.51";
            }
            else if (v.Version.Major == 4)
            {
                if (v.Platform == PlatformID.Win32Windows)
                {
                    if (v.Version.Minor == 0)
                    {
                        name = "Windows 95";
                    }
                    else if (v.Version.Minor == 10)
                    {

                        name = "Windows 98";
                    }
                    else if (v.Version.Minor == 90)
                    {

                        name = "Windows ME";
                    }
                }
                else if (v.Platform == PlatformID.Win32NT)
                {
                    name = "Windows NT 4.0";
                }
            }
            else if (v.Version.Major == 5)
            {
                if (v.Version.Minor == 0)
                {
                    name = "Windows 2000";
                }
                else if (v.Version.Minor == 1)
                {
                    name = "Windows XP";
                }
                else if (v.Version.Minor == 2)
                {
                    name = "Windows .NET Server";
                }
            }
            else if (v.Version.Major == 6)
            {
                if (v.Version.Minor == 0)
                {
                    name = isWorkstation ? "Windows Vista" : "Windows 2008";
                }
                else if (v.Version.Minor == 1)
                {
                    name = isWorkstation ? "Windows 7" : "Windows 2008 R2";
                }
                else if (v.Version.Minor == 2)
                {
                    name = isWorkstation ? "Windows 8" : "Windows 2012";
                }
                else if (v.Version.Minor == 3)
                {
                    name = isWorkstation ? "Windows 8.1" : "Windows 2012 R2";
                }
            }
            else if (v.Version.Major == 6)
            {
                // TODO: make the assembly manifest indicate that this is windows 8.1+ compat
                name = isWorkstation ? "Windows 10" : "Windows 2016";
            }

            var os = default(string);
            if (string.IsNullOrEmpty(v.ServicePack))
            {
                os = string.Format("{0} (Build {1})", name, v.Version.Build);
            }
            else
            {
                os = string.Format("{0} (Build {1}, {2})", name, v.Version.Build, v.ServicePack);
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("OS String: {0}", os));
#endif
            return os;
        }
    }
}
