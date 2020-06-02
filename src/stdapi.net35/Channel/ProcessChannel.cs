using Met.Core.Extensions;
using Met.Core.Proto;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Met.Stdapi.Channel
{
    public class ProcessChannel : Core.Channel
    {
        private readonly Process process;
        private bool interactive = false;
        private readonly Thread outputThread;
        private readonly Thread errorThread;
        private readonly Semaphore interactiveSemaphore;

        public ProcessChannel(Core.ChannelManager channelManager, Process process)
            : base(channelManager)
        {
            this.process = process;
            this.process.EnableRaisingEvents = true;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;

            this.outputThread = new Thread(new ParameterizedThreadStart(this.OutputReceived));
            this.errorThread = new Thread(new ParameterizedThreadStart(this.OutputReceived));

            // A mutex for both threads that handle stdout/stderr
            this.interactiveSemaphore = new Semaphore(0, 2);
        }

        private void OutputReceived(object state)
        {
            var stream = (StreamReader)state;

            while(!this.process.HasExited && !stream.EndOfStream)
            {
                if (!this.interactive)
                {
                    this.interactiveSemaphore.WaitOne(500);
                }

                if (this.interactive)
                {
                    using (var buffer = new MemoryStream())
                    {
                        while (stream.Peek() != -1)
                        {
                            var c = stream.Read();
                            buffer.WriteByte((byte)c);
                        }

                        // write out a packet
                        var packet = new Packet("core_channel_write");
                        packet.Add(TlvType.ChannelId, this.ChannelId);
                        packet.Add(TlvType.ChannelData, buffer.ToArray());

                        this.ChannelManager.Dispatch(packet);
                    }
                }
            }
        }

        public void ProcessStarted()
        {
            this.process.Exited += this.ProcessExited;
            this.outputThread.Start(this.process.StandardOutput);
            this.errorThread.Start(this.process.StandardError);
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            this.outputThread.Abort();
            this.errorThread.Abort();
            base.FireClosedEvent();
        }

        public override void Interact(bool interact)
        {
            var fireEvent = interact && !this.interactive;
            this.interactive = interact;
            if (fireEvent)
            {
                // Indicate to both threads that they can continue processing
                // as we have gone interactive
                this.interactiveSemaphore.Release(2);
            }
        }

        public override void Close()
        {
            this.process.Kill();
        }

        protected override PacketResult WriteInternal(byte[] data, int bytesToWrite, out int bytesWritten)
        {
            var result = PacketResult.InvalidData;
            bytesWritten = 0;

            if (data != null)
            {
                this.process.StandardInput.Write(this.process.StandardOutput.CurrentEncoding.GetString(data));
                bytesWritten = data.Length;
                result = PacketResult.Success;
            }

            return result;
        }
    }
}
