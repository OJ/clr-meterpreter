using Met.Core;
using Met.Core.Extensions;
using Met.Core.Proto;
using System.IO;

namespace Met.Stdapi.Channel
{
    public class FileChannel : Core.Channel
    {
        private readonly FileStream fileStream;

        private FileChannel(ChannelManager channelManager, FileStream fileStream)
            : base(channelManager)
        {
            this.fileStream = fileStream;
        }

        public static FileChannel Create(Core.ChannelManager channelManager, Packet request, Packet response)
        {
            var filePath = request.Tlvs.TryGetTlvValueAsString(TlvType.StdapiFilePath);
            var mode = request.Tlvs.TryGetTlvValueAsString(TlvType.StdapiFileMode);

            var fileMode = default(FileMode);
            var fileAccess = default(FileAccess);

            switch(mode)
            {
                case "abb":
                    {
                        fileMode = FileMode.Append;
                        fileAccess = FileAccess.Write;
                        break;
                    }
                case "rbb":
                    {
                        fileMode = FileMode.Open;
                        fileAccess = FileAccess.Read;
                        break;
                    }
                case "wbb":
                    {
                        fileMode = FileMode.Create;
                        fileAccess = FileAccess.Write;
                        break;
                    }
                default:
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Unable to handle file mode: {0}", mode));
                        return null;
                    }
            }

            try
            {
                var file = File.Open(filePath, fileMode, fileAccess);
                return new FileChannel(channelManager, file);
            }
            catch
            {
                return null;
            }
        }

        public override PacketResult IsEof(Packet request, Packet response)
        {
            try
            {
                response.Add(TlvType.Bool, this.fileStream.Length == this.fileStream.Position);
                return PacketResult.Success;
            }
            catch
            {
                return PacketResult.BadArguments;
            }
        }

        public override PacketResult Tell(Packet request, Packet response)
        {
            try
            {
                response.Add(TlvType.SeekPos, (uint)this.fileStream.Position);
                return PacketResult.Success;
            }
            catch
            {
                return PacketResult.BadArguments;
            }
        }

        public override void Close()
        {
            fileStream.Close();
        }

        protected override PacketResult WriteInternal(byte[] data, int bytesToWrite, out int bytesWritten)
        {
            try
            {
                this.fileStream.Write(data, 0, bytesToWrite);
                bytesWritten = bytesToWrite;
                return PacketResult.Success;
            }
            catch
            {
                bytesWritten = 0;
                return PacketResult.CallNotImplemented;
            }
        }

        protected override PacketResult ReadInternal(byte[] buffer, out int bytesRead)
        {
            bytesRead = this.fileStream.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                return PacketResult.Success;
            }

            return PacketResult.BadArguments;
        }
    }
}
