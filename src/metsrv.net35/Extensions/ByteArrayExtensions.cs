using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Met.Core.Extensions
{
    public static class ByteArrayExtensions
    {
        public static void Xor(this byte[] target, byte[] xorKey)
        {
            for (int i = 0; i < target.Length; ++i)
            {
                target[i] ^= xorKey[i % xorKey.Length];
            }
        }
    }
}
