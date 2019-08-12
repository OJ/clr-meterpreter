using Met.Core;
using Met.Core.Extensions;
using Met.Core.Proto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

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
            manager.RegisterFunction(name, "stdapi_fs_delete_file", false, this.DeleteFile);
            manager.RegisterFunction(name, "stdapi_fs_file_move", false, (req, resp) => FileOperation(req, resp, File.Move));
            manager.RegisterFunction(name, "stdapi_fs_file_copy", false, (req, resp) => FileOperation(req, resp, File.Copy));
            manager.RegisterFunction(name, "stdapi_fs_ls", false, this.ListEntries);
            manager.RegisterFunction(name, "stdapi_fs_stat", false, this.Stat);
            manager.RegisterFunction(name, "stdapi_fs_file_expand_path", false, this.ExpandPath);
            manager.RegisterFunction(name, "stdapi_fs_search", false, this.Search);
            manager.RegisterFunction(name, "stdapi_fs_md5", false, (req, resp) => this.Checksum(req, resp, () => MD5.Create()));
            manager.RegisterFunction(name, "stdapi_fs_sha1", false, (req, resp) => this.Checksum(req, resp, () => new SHA1Managed()));
            manager.RegisterFunction(name, "stdapi_fs_mount_show", false, this.ShowMount);
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

        private InlineProcessingResult ShowMount(Packet request, Packet response)
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                var mountTlv = response.AddGroup(TlvType.StdapiMount);

                mountTlv.Add(TlvType.StdapiMountName, drive.Name);
                mountTlv.Add(TlvType.StdapiMountType, (uint)drive.DriveType);

                if (drive.IsReady)
                {
                    mountTlv.Add(TlvType.StdapiMountSpaceTotal, drive.TotalSize);
                    mountTlv.Add(TlvType.StdapiMountSpaceUser, drive.AvailableFreeSpace);
                    mountTlv.Add(TlvType.StdapiMountSpaceFree, drive.TotalFreeSpace);
                }

                if (drive.DriveType == DriveType.Network)
                {
                    var universalName = GetUniversalName(drive.Name);
                    mountTlv.Add(TlvType.StdapiMountUncPath, universalName);
                }
            }

            response.Result = PacketResult.Success;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult Checksum(Packet request, Packet response, Func<HashAlgorithm> hashGenerator)
        {
            var fileTlv = request.Tlvs.TryGetTlv(TlvType.StdapiFilePath);
            var result = PacketResult.BadArguments;

            fileTlv.Pokemon(f =>
            {
                var file = Environment.ExpandEnvironmentVariables(f.ValueAsString());
                using (var hash = hashGenerator())
                using (var stream = File.OpenRead(file))
                {
                    response.Add(TlvType.StdapiFileHash, hash.ComputeHash(stream));
                }
                result = PacketResult.Success;
            });

            response.Result = result;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult Stat(Packet request, Packet response)
        {
            var fileTlv = request.Tlvs.TryGetTlv(TlvType.StdapiFilePath);
            var result = PacketResult.BadArguments;

            fileTlv.Pokemon(f =>
            {
                var s = FileStat(Environment.ExpandEnvironmentVariables(f.ValueAsString()));
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
                var dir = Environment.ExpandEnvironmentVariables(dirTlv.ValueAsString());
                foreach (var entry in EnumerateFileSystemEntries(dir))
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

        private IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            var fileName = Path.GetFileName(path);
            var dirName = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(dirName))
            {
                dirName = Environment.CurrentDirectory;
            }

            if (fileName.Contains("*"))
            {
                return Directory.GetFileSystemEntries(dirName, fileName);
            }
            else
            {
                return Directory.GetFileSystemEntries(path);
            }
        }

        private InlineProcessingResult ExpandPath(Packet request, Packet response)
        {
            var fileTlv = request.Tlvs.TryGetTlv(TlvType.StdapiFilePath);
            var result = PacketResult.BadArguments;

            fileTlv.Pokemon(f =>
            {
                response.Add(TlvType.StdapiFilePath, Environment.ExpandEnvironmentVariables(f.ValueAsString()));
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
                Environment.CurrentDirectory = Environment.ExpandEnvironmentVariables(d.ValueAsString());
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

        private InlineProcessingResult Search(Packet request, Packet response)
        {
            // TODO: fill this in when I can be bothered
            response.Result = PacketResult.CallNotImplemented;
            return InlineProcessingResult.Continue;
        }

        private InlineProcessingResult DeleteDirectory(Packet request, Packet response)
        {
            var dirTlv = request.Tlvs.TryGetTlv(TlvType.StdapiDirectoryPath);
            var result = PacketResult.BadArguments;

            if (dirTlv != null)
            {
                var dir = Environment.ExpandEnvironmentVariables(dirTlv.ValueAsString());
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

        private InlineProcessingResult FileOperation(Packet request, Packet response, Action<string, string> fileOp)
        {
            var oldFileTlv = request.Tlvs.TryGetTlv(TlvType.StdapiFileName);
            var newFileTlv = request.Tlvs.TryGetTlv(TlvType.StdapiFilePath);
            var result = PacketResult.BadArguments;

            if (oldFileTlv != null && newFileTlv != null)
            {
                var oldFile = Environment.ExpandEnvironmentVariables(oldFileTlv.ValueAsString());
                var newFile = Environment.ExpandEnvironmentVariables(newFileTlv.ValueAsString());
                if (File.Exists(oldFile))
                {
                    try
                    {
                        fileOp(oldFile, newFile);
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

        private InlineProcessingResult DeleteFile(Packet request, Packet response)
        {
            var fileTlv = request.Tlvs.TryGetTlv(TlvType.StdapiFilePath);
            var result = PacketResult.BadArguments;

            if (fileTlv != null)
            {
                var file = Environment.ExpandEnvironmentVariables(fileTlv.ValueAsString());
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
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
                var dir = Environment.ExpandEnvironmentVariables(dirTlv.ValueAsString());
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

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
            string lpszLongPath,
            [MarshalAs(UnmanagedType.LPTStr)]
            StringBuilder lpszShortPath,
            int cchBuffer);

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

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int WNetGetUniversalName(
            string lpLocalPath,
            [MarshalAs(UnmanagedType.U4)]
            int dwInfoLevel,
            IntPtr lpBuffer,
            [MarshalAs(UnmanagedType.U4)]
            ref int lpBufferSize);
 
        private const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
        private const int ERROR_MORE_DATA = 234;
        private const int NOERROR = 0;

        private static string GetUniversalName(string localPath)
        {
            var universalName = default(string);
            var buffer = IntPtr.Zero;

            try
            {
                int size = 0;

                int apiRetVal = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, (IntPtr)IntPtr.Size, ref size);

                if (apiRetVal != ERROR_MORE_DATA)
                {
                    throw new Win32Exception(apiRetVal);
                }

                buffer = Marshal.AllocCoTaskMem(size);

                apiRetVal = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);

                if (apiRetVal != NOERROR)
                {
                    throw new Win32Exception(apiRetVal);
                }

                universalName = Marshal.PtrToStringAuto(new IntPtr(buffer.ToInt64() + IntPtr.Size), size);
                universalName = universalName.Substring(0, universalName.IndexOf('\0'));
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(buffer);
                }
            }

            return universalName;
        }
    }
}
