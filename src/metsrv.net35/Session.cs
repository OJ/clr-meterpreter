using Met.Core.Extensions;
using System;
using System.IO;

namespace Met.Core
{
    public class Session
    {
        private const int UUID_SIZE = 16;

        public byte[] SessionGuid { get; set; }
        public byte[] SessionUuid { get; set; }
        public UInt32 ExitFunc { get; set; }
        public DateTime Expiry { get; set; }

        public Session(BinaryReader reader)
        {
            // Skip the first 4 bytes, because this contains a handle that .NET meterpreter
            // doesn't care about.
            reader.ReadQword();

            this.ExitFunc = reader.ReadUInt32();
            this.Expiry = DateTime.UtcNow.AddSeconds(reader.ReadUInt32());
            this.SessionUuid = reader.ReadBytes(UUID_SIZE);
            this.SessionGuid = reader.ReadBytes(Guid.Empty.ToByteArray().Length);
        }
    }
}
