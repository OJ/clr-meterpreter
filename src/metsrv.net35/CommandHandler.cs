using Met.Core.Proto;
using System.Management;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Met.Core
{
    public class CommandHandler
    {
        //[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public extern static bool GetVolumeInformation(
        //    string rootPathName,
        //    StringBuilder volumeNameBuffer,
        //    int volumeNameSize,
        //    out uint volumeSerialNumber,
        //    out uint maximumComponentLength,
        //    out uint fileSystemFlags,
        //    StringBuilder fileSystemNameBuffer,
        //    int nFileSystemNameSize);

        public void Register(PluginManager manager)
        {
            manager.RegisterFunction(string.Empty, "core_machine_id", false, this.CoreMachineId);
        }

        private static bool GetVolumeInformation(
            string rootPathName,
            StringBuilder volumeNameBuffer,
            int volumeNameSize,
            out uint volumeSerialNumber,
            out uint maximumComponentLength,
            out uint fileSystemFlags,
            StringBuilder fileSystemNameBuffer,
            int nFileSystemNameSize)
        {
            var assemblyName = new AssemblyName("jfkldasljk");
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("fjdklsfjsa");
            var typeBuilder = moduleBuilder.DefineType("fdklsfjakl", TypeAttributes.Public);

            var dllImportConstructor = typeof(DllImportAttribute).GetConstructor(new Type[] { typeof(string) });
            var dllImportBuilder = new CustomAttributeBuilder(dllImportConstructor, new object[] { "Kernel32.dll" });

            // Possible MethodAttributes.PinvokeImpl
            var methodBuilder = typeBuilder.DefinePInvokeMethod("GetVolumeInformation", "Kernel32.dll", MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard, typeof(bool),
                new Type[] { typeof(string), typeof(StringBuilder), typeof(int), typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType(),
                    typeof(uint).MakeByRefType(), typeof(StringBuilder), typeof(int) },
                CallingConvention.Winapi, CharSet.Auto);

            methodBuilder.SetCustomAttribute(dllImportBuilder);

            volumeSerialNumber = 0;
            maximumComponentLength = 0;
            fileSystemFlags = 0;

            var args = new object[] { rootPathName,
            volumeNameBuffer, volumeNameSize, volumeSerialNumber, maximumComponentLength, fileSystemFlags, fileSystemNameBuffer,
            nFileSystemNameSize
            };

            var type = typeBuilder.CreateType();
            var result = (bool)type.GetMethod("GetVolumeInformation").Invoke(null, args);
            volumeSerialNumber = (uint)args[3];
            maximumComponentLength = (uint)args[4];
            fileSystemFlags = (uint)args[5];
            return result;
        }

        private InlineProcessingResult CoreMachineId(Packet request, Packet response)
        {
            var sysDrive = Environment.SystemDirectory.Substring(0, 3);

            var volname = new StringBuilder(261);
            var fsname = new StringBuilder(261);
            uint serialNumber = 0, maxLength = 0, flags = 0;

            GetVolumeInformation(sysDrive, volname, volname.Capacity, out serialNumber, out maxLength, out flags, fsname, fsname.Capacity);

            var machineId = string.Format("{0,04:x}-{1,04:x}:{2}", (ushort)(serialNumber >> 16), (ushort)serialNumber, Environment.MachineName);

            response.Add(TlvType.MachineId, machineId);
            response.Result = PacketResult.Success;

            return InlineProcessingResult.Continue;
        }
    }
}
