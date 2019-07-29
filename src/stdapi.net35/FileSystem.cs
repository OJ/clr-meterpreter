using Met.Core;
using Met.Core.Proto;
using Met.Core.Extensions;
using System.Runtime.InteropServices;

namespace Met.Stdapi
{
    public class FileSystem
    {
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
                var path = f.ValueAsString();

                var fi = new System.IO.FileInfo(path);

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
                foreach (var entry in System.IO.Directory.GetFileSystemEntries(dirTlv.ValueAsString()))
                {
                    response.Add(TlvType.StdapiFileName, entry);
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
                System.Environment.CurrentDirectory = d.ValueAsString();
                result = PacketResult.Success;
            });

            response.Result = result;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult GetCurrentDirectory(Packet request, Packet response)
        {
            response.Add(TlvType.StdapiDirectoryPath, System.Environment.CurrentDirectory);
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
                if (System.IO.Directory.Exists(dir))
                {
                    try
                    {
                        System.IO.Directory.Delete(dir);
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
                if (!System.IO.Directory.Exists(dir))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(dir);
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
            response.Add(TlvType.String, System.IO.Path.DirectorySeparatorChar.ToString());
            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }
    }
}
