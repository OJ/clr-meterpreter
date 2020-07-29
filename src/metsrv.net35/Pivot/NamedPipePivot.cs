using Met.Core.Extensions;
using Met.Core.Proto;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Threading;

namespace Met.Core.Pivot
{
    public class PacketBody
    {
        private readonly PacketHeader header;

        public byte[] Buffer { get; private set; }
        public int Position { get; set; }

        public PacketBody(PacketHeader header)
        {
            this.header = header;
            this.header.Prepare();
            this.Buffer = new byte[this.header.BodyLength - 8];
            this.Position = 0;
        }

        public Packet ToPacket()
        {
            this.header.Unprepare();
            using (var memStream = new MemoryStream())
            {
                memStream.Write(this.header.Buffer, 0, this.header.Buffer.Length);
                memStream.Write(this.Buffer, 0, this.Buffer.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                using (var binaryReader = new BinaryReader(memStream))
                {
                    return new Packet(binaryReader);
                }
            }
        }
    }
    public class PacketHeader
    {
        private byte[] xorKey;

        public byte[] Buffer { get; private set; }
        public int Position { get; set; }

        public uint BodyLength
        {
            get
            {
                return (UInt32)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(this.Buffer, this.Buffer.Length - 8));
            }
        }

        public PacketHeader()
        {
            this.Buffer = new byte[Packet.HEADER_SIZE];
            this.Position = 0;
        }

        public void Prepare()
        {
            this.xorKey = new byte[4];
            Array.Copy(this.Buffer, this.xorKey, this.xorKey.Length);
            this.Buffer.Xor(this.xorKey);
        }

        public void Unprepare()
        {
            this.Buffer.Xor(this.xorKey);
        }
    }

    public class NamedPipePivot : Pivot
    {
        private readonly NamedPipeServerStream server;
        private bool established = false;
        private Guid sessionId = Guid.Empty;
        private string getSessionGuidReqId = null;

        public NamedPipePivot(IPacketDispatcher packetDispacher, NamedPipeServerStream server, byte[] stageData)
            : base(packetDispacher)
        {
            this.server = server;
            if (stageData != null && stageData.Length > 0)
            {
                using (var memStream = new MemoryStream(stageData.Length + 4))
                using (var writer = new BinaryWriter(memStream))
                {
                    writer.Write(stageData.Length);
                    writer.Write(stageData);
                    Write(memStream.ToArray());
                }
            }

            EstablishSession();
            ReadHeaderAsync();
        }

        private void EstablishSession()
        {
            var packet = new Packet("core_get_session_guid");
            this.getSessionGuidReqId = packet.RequestId;
            Write(packet);
        }

        private void ReadHeaderAsync()
        {
            var packetHeader = new PacketHeader();
            this.server.BeginRead(packetHeader.Buffer, 0, packetHeader.Buffer.Length, HeaderDataReceived, packetHeader);
        }

        private void HeaderDataReceived(IAsyncResult result)
        {
            var packetHeader = (PacketHeader)result.AsyncState;
            var bytesRead = this.server.EndRead(result);

            if(bytesRead > 0)
            {
                packetHeader.Position += bytesRead;

                var bytesLeft = packetHeader.Buffer.Length - packetHeader.Position ;
                if (bytesLeft > 0)
                {
                    this.server.BeginRead(packetHeader.Buffer, packetHeader.Position, bytesLeft, HeaderDataReceived, packetHeader);
                }
                else
                {
                    // The header has been received, let's receive the body of the packet next.
                    var packetBody = new PacketBody(packetHeader);
                    this.server.BeginRead(packetBody.Buffer, 0, packetBody.Buffer.Length, BodyDataReceived, packetBody);
                }
            }
        }

        private void BodyDataReceived(IAsyncResult result)
        {
            var packetBody = (PacketBody)result.AsyncState;
            var bytesRead = this.server.EndRead(result);

            if(bytesRead > 0)
            {
                packetBody.Position += bytesRead;

                var bytesLeft = packetBody.Buffer.Length - packetBody.Position ;
                if (bytesLeft > 0)
                {
                    this.server.BeginRead(packetBody.Buffer, packetBody.Position, bytesLeft, HeaderDataReceived, packetBody);
                }
                else
                {
                    // We now have a full packet.
                    var packet = packetBody.ToPacket();

                    if (!this.established && packet.RequestId == this.getSessionGuidReqId)
                    {
                        var guid = packet.Tlvs.TryGetTlvValueAsRaw(TlvType.SessionGuid);
                    }
                }
            }
        }

        private void Write(Packet packet)
        {
            Write(packet.ToRaw(this.sessionId));
        }

        private void Write(byte[] data)
        {
            //this.server.BeginWrite(data, 0, data.Length, DataWritten, null);
            this.server.Write(data, 0, data.Length);
        }

        private void DataWritten(IAsyncResult result)
        {
            this.server.EndWrite(result);
        }
    }
}
