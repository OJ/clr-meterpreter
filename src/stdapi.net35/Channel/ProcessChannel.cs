using Met.Core.Proto;
using System;
using System.Diagnostics;
using Met.Core.Extensions;
using System.Text;
using System.IO;
using System.Threading;
using System.IO.Pipes;
using Microsoft.Win32.SafeHandles;

namespace Met.Stdapi.Channel
{
    public class ProcessChannel : Core.Channel
    {
        private readonly Process process;
        private bool interactive = false;
        private readonly byte[] outputBuffer = null;
        private bool closing = false;
        private Thread outputThread;

        public ProcessChannel(Action<Packet> packetDispatcher, Process process)
            : base(packetDispatcher)
        {
            this.process = process;
            this.outputBuffer = new byte[ushort.MaxValue];

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
        }

        public void ProcessStarted()
        {
            this.outputThread = new Thread(new ParameterizedThreadStart(this.HandleProcessOutput));
            this.outputThread.Start(this.process.StandardOutput.BaseStream);
        }

        private void HandleProcessOutput(object state)
        {
            var buffer = new byte[ushort.MaxValue];
            var index = 0;
            var fileStream = (FileStream)state;
            var handle = new SafePipeHandle(fileStream.SafeFileHandle.DangerousGetHandle(), false);

            using (var stream = new NamedPipeClientStream(PipeDirection.Out, fileStream.IsAsync, true, handle))
            {
                while (true)
                {
                    var bytesRead = stream.Read(buffer, 0, buffer.Length);
                    var c = stream.ReadByte();
                    while (c != -1 && index < buffer.Length)
                    {
                        if (this.interactive)
                        {
                            buffer[index++] = (byte)c;
                        }
                        c = stream.ReadByte();
                    }

                    if (index > 0)
                    {
                        if (this.interactive)
                        {
                            var packet = new Packet("core_channel_write");
                            packet.Add(TlvType.ChannelId, this.ChannelId);
                            packet.Add(TlvType.ChannelData, buffer, index);
                            this.PacketDispatcher(packet);
                        }
                        index = 0;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
        }

        private void OutputReadCompleted(IAsyncResult result)
        {
            var processClosed = false;

            try
            {
                int bytesRead = this.process.StandardInput.BaseStream.EndRead(result);

                if (bytesRead > 0)
                {
                    if (this.interactive)
                    {
                        var packet = new Packet("core_channel_write");
                        packet.Add(TlvType.ChannelId, this.ChannelId);
                        packet.Add(TlvType.ChannelData, this.outputBuffer, bytesRead);
                        this.PacketDispatcher(packet);
                    }
                }
                else
                {
                    processClosed = true;
                }

            }
            catch(Exception e)
            {
                Debug.WriteLine(string.Format("ProcessChannel Exception: {0}", e.Message));
                processClosed = true;
            }

            if (processClosed)
            {
                if (!this.closing)
                {
                    this.FireClosedEvent();
                    this.Close();
                }
            }
            else
            {
                //this.BeginReadOutput();
            }
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                var data = this.process.StandardOutput.CurrentEncoding.GetBytes(Environment.NewLine + e.Data);
                var packet = new Packet("core_channel_write");
                packet.Add(TlvType.ChannelId, this.ChannelId);
                packet.Add(TlvType.ChannelData, data, data.Length);
                this.PacketDispatcher(packet);
            }
        }

        public override void Interact(bool interact)
        {
            //if (interact && !this.interactive)
            //{
            //    process.OutputDataReceived += OutputDataReceived;
            //    process.ErrorDataReceived += OutputDataReceived;
            //}
            //else if (!interact && this.interactive)
            //{
            //    process.OutputDataReceived -= OutputDataReceived;
            //    process.ErrorDataReceived -= OutputDataReceived;
            //}

            this.interactive = interact;
        }

        public override void Close()
        {
            this.process.Kill();
        }

        public override PacketResult Write(Packet request, Packet response, out int bytesWritten)
        {
            var result = PacketResult.InvalidData;
            var bytes = request.Tlvs.TryGetTlvValueAsRaw(TlvType.ChannelData);
            bytesWritten = 0;

            if (bytes != null)
            {
                this.process.StandardInput.Write(this.process.StandardOutput.CurrentEncoding.GetString(bytes));
                bytesWritten = bytes.Length;
                result = PacketResult.Success;
            }

            return result;
        }
    }
}
