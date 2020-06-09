using System;
using System.IO;
using System.IO.Pipes;

namespace Met.Core.Pivot
{
    public class NamedPipePivot : Pivot
    {
        private readonly NamedPipeServerStream server;
        private readonly byte[] readBuffer;

        public NamedPipePivot(IPacketDispatcher packetDispacher, NamedPipeServerStream server, byte[] stageData)
            : base(packetDispacher)
        {
            this.readBuffer = new byte[ushort.MaxValue];
            this.server = server;

            if (stageData != null && stageData.Length > 0)
            {
                this.server.BeginWrite(stageData, 0, stageData.Length, this.DataWritten, stageData);
            }

            ReadAsync();
        }

        private void ReadAsync()
        {
            this.server.BeginRead(this.readBuffer, 0, this.readBuffer.Length, DataRead, new MemoryStream());
        }

        private void DataRead(IAsyncResult result)
        {
            var bytesRead = this.server.EndRead(result);
            int x = 0;
        }

        private void DataWritten(IAsyncResult result)
        {
            this.server.EndWrite(result);
        }
    }
}
