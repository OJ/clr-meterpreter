using Met.Core.Extensions;
using Met.Core.Proto;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Met.Core.Pivot
{
    public class NamedPipePivot : Pivot
    {
        private readonly NamedPipeServerStream server;
        private bool established = false;
        private Guid sessionId = Guid.Empty;
        private string getSessionGuidReqId = null;
        private Thread readerThread = null;
        private BinaryWriter writer = null;
        private BinaryReader reader = null;

        public NamedPipePivot(IPacketDispatcher packetDispacher, NamedPipeServerStream server, byte[] stageData)
            : base(packetDispacher)
        {
            this.readerThread = new Thread(new ThreadStart(this.ReadAsync));
            this.server = server;
            //this.writer = new BinaryWriter(this.server);
            this.reader = new BinaryReader(this.server);

            if (stageData != null && stageData.Length > 0)
            {
                using (var memStream = new MemoryStream(stageData.Length + 4))
                using (var writer = new BinaryWriter(memStream))
                {
                    Write(memStream.ToArray());
                }
            }

            EstablishSession();
            this.readerThread.Start();
        }

        private void EstablishSession()
        {
            var packet = new Packet("core_get_session_guid");
            this.getSessionGuidReqId = packet.RequestId;
            Write(packet);
        }

        private void ReadAsync()
        {
            var packet = new Packet(this.reader);
            if (!this.established && packet.RequestId == this.getSessionGuidReqId)
            {
                var guid = packet.Tlvs.TryGetTlvValueAsRaw(TlvType.SessionGuid);
            }
        }

        private void Write(Packet packet)
        {
            Write(packet.ToRaw(this.sessionId));
        }

        private void Write(byte[] data)
        {
            new Thread(new ThreadStart(() =>
            {
                //this.writer.Write(data);
                this.server.Write(data, 0, data.Length);
            })).Start();
        }
    }
}
