using System;
using System.IO.Pipes;

namespace Met.Core.Pivot
{
    public class NamedPipePivotListener : PivotListener
    {
        private readonly string pipeName;
        private readonly byte[] stageData;

        public NamedPipePivotListener(IPacketDispatcher packetDispatcher, Guid id, string pipeName, byte[] stageData)
            : base(packetDispatcher, id)
        {
            this.pipeName = pipeName;
            this.stageData = stageData;
            WaitForConnection();
        }

        private void WaitForConnection()
        {
            var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            server.BeginWaitForConnection(this.HandleConnection, server);
        }

        private void HandleConnection(IAsyncResult result)
        {
            var server = (NamedPipeServerStream)result.AsyncState;
            try
            {
                server.EndWaitForConnection(result);
                var pivot = new NamedPipePivot(this.PacketDispatcher, server, this.stageData);
                FirePivotAdded(pivot);
            }
            catch
            {
            }

            WaitForConnection();
        }
    }
}
