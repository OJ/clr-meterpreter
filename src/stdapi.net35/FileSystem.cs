using Met.Core;
using Met.Core.Extensions;
using Met.Core.Proto;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Met.Stdapi
{
    public class FileSystem
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
            string lpszLongPath,
            [MarshalAs(UnmanagedType.LPTStr)]
            StringBuilder lpszShortPath,
            int cchBuffer);

        public void Register(string name, PluginManager manager)
        {
            manager.RegisterFunction(name, "stdapi_fs_separator", false, this.GetFileSystemSeperator);
            manager.RegisterFunction(name, "stdapi_fs_getwd", false, this.GetCurrentDirectory);
            manager.RegisterFunction(name, "stdapi_fs_chdir", false, this.SetCurrentDirectory);
            manager.RegisterFunction(name, "stdapi_fs_mkdir", false, this.CreateDirectory);
            manager.RegisterFunction(name, "stdapi_fs_delete_dir", false, this.DeleteDirectory);
            manager.RegisterFunction(name, "stdapi_fs_ls", false, this.ListEntries);
            manager.RegisterFunction(name, "stdapi_fs_stat", false, this.Stat);
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        private struct MeterpStat
        {
            public uint st_dev;
            public uint st_mode;
            public uint st_nlink;
            public uint st_uid;
            public uint st_gid;
            public uint st_rdev;
            public ulong st_ino;
            public ulong st_size;
            public ulong st_atime;
            public ulong st_mtime;
            public ulong st_ctime;
        };

        private InlineProcessingResult Stat(Packet request, Packet response)
        {
            var fileTlv = request.Tlvs.TryGetTlv(TlvType.StdapiFilePath);
            var result = PacketResult.BadArguments;

            fileTlv.Pokemon(f =>
            {
                var s = FileStat(f.ValueAsString());
                response.Add(TlvType.StdapiStatBuf, s);
                result = PacketResult.Success;
            });

            response.Result = result;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult ListEntries(Packet request, Packet response)
        {
            var dirTlv = request.Tlvs.TryGetTlv(TlvType.StdapiDirectoryPath);
            var result = PacketResult.BadArguments;

            dirTlv.Pokemon(d =>
            {
                var dir = dirTlv.ValueAsString();
                foreach (var entry in Directory.GetFileSystemEntries(dir))
                {
                    var shortName = ToShortName(entry);
                    response.Add(TlvType.StdapiFileName, Path.GetFileName(entry));
                    response.Add(TlvType.StdapiFileShortName, Path.GetFileName(shortName));
                    response.Add(TlvType.StdapiFilePath, entry);
                    response.Add(TlvType.StdapiStatBuf, FileStat(entry));
                }

                result = PacketResult.Success;
            });

            response.Result = result;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult SetCurrentDirectory(Packet request, Packet response)
        {
            var dirTlv = request.Tlvs.TryGetTlv(TlvType.StdapiDirectoryPath);
            var result = PacketResult.BadArguments;

            dirTlv.Pokemon(d =>
            {
                Environment.CurrentDirectory = d.ValueAsString();
                result = PacketResult.Success;
            });

            response.Result = result;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult GetCurrentDirectory(Packet request, Packet response)
        {
            response.Add(TlvType.StdapiDirectoryPath, Environment.CurrentDirectory);
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult DeleteDirectory(Packet request, Packet response)
        {
            var dirTlv = request.Tlvs.TryGetTlv(TlvType.StdapiDirectoryPath);
            var result = PacketResult.BadArguments;

            if (dirTlv != null)
            {
                var dir = dirTlv.ValueAsString();
                if (Directory.Exists(dir))
                {
                    try
                    {
                        Directory.Delete(dir);
                        result = PacketResult.Success;
                    }
                    catch
                    {
                        // We don't care why this fails
                    }
                }
            }

            response.Result = result;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult CreateDirectory(Packet request, Packet response)
        {
            var dirTlv = request.Tlvs.TryGetTlv(TlvType.StdapiDirectoryPath);
            var result = PacketResult.BadArguments;

            if (dirTlv != null)
            {
                var dir = dirTlv.ValueAsString();
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                        result = PacketResult.Success;
                    }
                    catch
                    {
                        // We don't care why this fails
                    }
                }
                else
                {
                    result = PacketResult.ErrorAlreadyExists;
                }
            }

            response.Result = result;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult GetFileSystemSeperator(Packet request, Packet response)
        {
            response.Add(TlvType.String, Path.DirectorySeparatorChar.ToString());
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private MeterpStat FileStat(string path)
        {
            var fi = new FileInfo(path);

            var s = new MeterpStat
            {
                st_mode = fi.ToMode(),
                st_ctime = fi.CreationTime.ToUnixTimestamp(),
                st_mtime = fi.LastWriteTime.ToUnixTimestamp(),
                st_atime = fi.LastAccessTime.ToUnixTimestamp(),
            };

            if (!fi.IsDirectory())
            {
                s.st_size = (ulong)fi.Length;
            }

            return s;
        }

        private string ToShortName(string longName)
        {
            // get the required length first, as this can be huge in cases
            // where the target has long file names enabled (which can be
            // up to 32767 chars long).
            var size = GetShortPathName(longName, null, 0);
            var sb = new StringBuilder((int)size + 1);
            GetShortPathName(longName, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
