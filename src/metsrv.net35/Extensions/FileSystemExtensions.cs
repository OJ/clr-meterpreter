using System.Collections.Generic;
using System.IO;

namespace Met.Core.Extensions
{
    public static class FileSystemExtensions
    {
        private static HashSet<string> executables = new HashSet<string>
        {
            ".exe",
            ".bat",
            ".cmd",
            ".com"
        };

        public static bool IsExecutable(this FileInfo info)
        {
            return executables.Contains(info.Extension.ToLower());
        }

        public static bool IsDirectory(this FileInfo info)
        {
            return info.Attributes.HasFlag(FileAttributes.Directory);
        }

        public static uint ToMode(this FileInfo info)
        {
            const uint _S_IFDIR = 0x4000u;
            const uint _S_IFREG = 0x8000u;

            var mode = 0u;

            if (info.IsDirectory())
            {
                mode |= _S_IFDIR | 0111u;
            }
            else
            {
                mode |= _S_IFREG;
            }

            if (info.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                mode |= 0444u;
            }
            else
            {
                mode |= 0666u;
            }

            if (info.IsExecutable())
            {
                mode |= 0111u;
            }

            return mode;
        }
    }
}

