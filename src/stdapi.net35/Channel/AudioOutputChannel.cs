using Met.Core.Proto;
using System.IO;

namespace Met.Stdapi.Channel
{
    public class AudioOutputChannel : Core.Channel
    {
        private readonly MemoryStream soundBuffer;

        public AudioOutputChannel(Core.ChannelManager channelManager)
            : base(channelManager)
        {
            this.soundBuffer = new MemoryStream();
        }

        protected override PacketResult WriteInternal(byte[] data, int bytesToWrite, out int bytesWritten)
        {
            this.soundBuffer.Write(data, 0, bytesToWrite);
            bytesWritten = bytesToWrite;
            return PacketResult.Success;
        }

        public override void Close()
        {
            this.soundBuffer.Seek(0, SeekOrigin.Begin);
            try
            {
                using (var player = new System.Media.SoundPlayer(this.soundBuffer))
                {
                    player.PlaySync();
                }
            }
            catch
            {
                // sound file might be invalid, so bail out quietly.
            }
            this.soundBuffer.Dispose();
        }

        public static AudioOutputChannel Create(Core.ChannelManager channelManager, Packet request, Packet response)
        {
            return new AudioOutputChannel(channelManager);
        }
    }
}
