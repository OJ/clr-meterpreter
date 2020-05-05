using Met.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Met.Core.Proto
{
    [Flags]
    public enum MetaType : UInt32
    {
        None = 0u,
        String = (1u << 16),
        Uint = (1u << 17),
        Raw = (1u << 18),
        Bool = (1u << 19),
        Qword = (1u << 20),
        Compressed = (1u << 29),
        Group = (1u << 30),
        Complex = (1u << 31),
        All = None | String | Uint | Raw | Bool | Qword | Compressed | Group | Complex
    }

    public enum TlvType : UInt32
    {
        // General/base type TLVs
        Any = MetaType.None,
        Method = MetaType.String | 1u,
        RequestId = MetaType.String | 2u,
        Exception = MetaType.Group | 3u,
        Result = MetaType.Uint | 4u,
        String = MetaType.String | 10u,
        Uint = MetaType.Uint | 11u,
        Bool = MetaType.Bool | 12u,
        Length = MetaType.Uint | 25u,
        Data = MetaType.Raw | 26u,
        Flags = MetaType.Uint | 27u,
        // Channel TLVs
        ChannelId = MetaType.Uint | 50u,
        ChannelType = MetaType.String | 51u,
        ChannelData = MetaType.Raw | 52u,
        ChannelDataGroup = MetaType.Group | 53u,
        ChannelClass = MetaType.Uint | 54u,
        ChannelParentId = MetaType.Uint | 55u,
        // File seeking TLVs
        SeekWhence = MetaType.Uint | 70u,
        SeekOffset = MetaType.Uint | 71u,
        SeekPos = MetaType.Uint | 72u,
        // Exception/error TLVs
        ExceptionCode = MetaType.Uint | 300u,
        ExceptionString = MetaType.String | 301u,
        // Migration TLVs
        LibraryPath = MetaType.String | 400u,
        TargetPath = MetaType.String | 401u,
        MigratePid = MetaType.Uint | 402u,
        MigratePayloadLen = MetaType.Uint | 403u,
        MigratePayload = MetaType.String | 404u,
        MigrateArch = MetaType.Uint | 405u,
        MigrateBaseAddr = MetaType.Uint | 407u,
        MigrateEntryPoint = MetaType.Uint | 408u,
        MigrateSocketPath = MetaType.Uint | 409u,
        MigrateStubLen = MetaType.Uint | 410u,
        MigrateStub = MetaType.Uint | 411u,
        // Transport TLVs
        TransType = MetaType.Uint | 430u,
        TransUrl = MetaType.String | 431u,
        TransUa = MetaType.String | 432u,
        TransCommTimeout = MetaType.Uint | 433u,
        TransSessExp = MetaType.Uint | 434u,
        TransCertHash = MetaType.Raw | 435u,
        TransProxyHost = MetaType.String | 436u,
        TransProxyUser = MetaType.String | 437u,
        TransProxyPass = MetaType.String | 438u,
        TransRetryTotal = MetaType.Uint | 439u,
        TransRetryWait = MetaType.Uint | 440u,
        TransHeaders = MetaType.String | 441u,
        TransGroup = MetaType.Group | 442u,
        // Identification/session TLVs
        MachineId = MetaType.String | 460u,
        Uuid = MetaType.Raw | 461u,
        SessionGuid = MetaType.Raw | 462u,
        // Packet encryption TLVs
        RsaPubKey = MetaType.String | 550u,
        SymKeyType = MetaType.Uint | 551u,
        SymKey = MetaType.Raw | 552u,
        EncSymKey = MetaType.Raw | 553u,
        // Pivot TLVs
        PivotId = MetaType.Raw | 650u,
        PivoteStageData = MetaType.Raw | 651u,
        PivoteStageDataLen = MetaType.Uint | 652u,
        PivotNamedPipeName = MetaType.String | 653u,
        // STDAPI stuff
        StdapiPlugin = 0u,
        StdapiComputerName = MetaType.String | (StdapiPlugin + 1040u),
        StdapiOperatingSystemName = MetaType.String | (StdapiPlugin + 1041u),
        StdapiUserName = MetaType.String | (StdapiPlugin + 1042u),
        StdapiArchitecture = MetaType.String | (StdapiPlugin + 1043u),
        StdapiLangSystem = MetaType.String | (StdapiPlugin + 1044u),
        StdapiSid = MetaType.String | (StdapiPlugin + 1045u),
        StdapiDomain = MetaType.String | (StdapiPlugin + 1046u),
        StdapiLoggedOnUserCount = MetaType.Uint | (StdapiPlugin + 1047u),
        StdapiLocalDateTime = MetaType.String | (StdapiPlugin + 1048u),
        StdapiEnvVariable = MetaType.String | (StdapiPlugin + 1100u),
        StdapiEnvValue = MetaType.String | (StdapiPlugin + 1101u),
        StdapiEnvGroup = MetaType.Group | (StdapiPlugin + 1102u),
        StdapiDirectoryPath = MetaType.String | (StdapiPlugin + 1200u),
        StdapiFileName = MetaType.String | (StdapiPlugin + 1201u),
        StdapiFilePath = MetaType.String | (StdapiPlugin + 1202u),
        StdapiFileMode = MetaType.String | (StdapiPlugin + 1203u),
        StdapiFileSize = MetaType.Uint | (StdapiPlugin + 1204u),
        StdapiFileShortName = MetaType.String | (StdapiPlugin + 1205u),
        StdapiFileHash = MetaType.Raw | (StdapiPlugin + 1206u),
        StdapiMount = MetaType.Group | (StdapiPlugin + 1207u),
        StdapiMountName = MetaType.String | (StdapiPlugin + 1208u),
        StdapiMountType = MetaType.Uint | (StdapiPlugin + 1209u),
        StdapiMountSpaceUser = MetaType.Qword | (StdapiPlugin + 1210u),
        StdapiMountSpaceTotal = MetaType.Qword | (StdapiPlugin + 1211u),
        StdapiMountSpaceFree = MetaType.Qword | (StdapiPlugin + 1212u),
        StdapiMountUncPath = MetaType.String | (StdapiPlugin + 1213u),
        StdapiStatBuf32 = MetaType.Complex | (StdapiPlugin + 1220u),
        StdapiStatBuf = MetaType.Complex | (StdapiPlugin + 1221u),
        StdapiInterfaceMtu = MetaType.Uint | (StdapiPlugin + 1402u),
        StdapiInterfaceFlags = MetaType.String | (StdapiPlugin + 1403u),
        StdapiInterfaceIndex = MetaType.Uint | (StdapiPlugin + 1404u),
        StdapiSubnet = MetaType.Raw | (StdapiPlugin + 1420u),
        StdapiNetmask = MetaType.Raw | (StdapiPlugin + 1421u),
        StdapiGateway = MetaType.Raw | (StdapiPlugin + 1422u),
        StdapiNetworkRoute = MetaType.Group | (StdapiPlugin + 1423u),
        StdapiIpPrefix = MetaType.Uint | (StdapiPlugin + 1424u),
        StdapiArpEntry = MetaType.Group | (StdapiPlugin + 1425u),
        StdapiIp = MetaType.Raw | (StdapiPlugin + 1430u),
        StdapiMacAddr = MetaType.Raw | (StdapiPlugin + 1431u),
        StdapiMacName = MetaType.String | (StdapiPlugin + 1432u),
        StdapiNetworkInterface = MetaType.Group | (StdapiPlugin + 1433u),
        StdapiIp6Scope = MetaType.Raw | (StdapiPlugin + 1434u),
        StdapiSubnetString = MetaType.String | (StdapiPlugin + 1440u),
        StdapiNetmaskString = MetaType.String | (StdapiPlugin + 1441u),
        StdapiGatewayString = MetaType.String | (StdapiPlugin + 1442u),
        StdapiRouteMetric = MetaType.Uint | (StdapiPlugin + 1443u),
        StdapiAddrType = MetaType.Uint | (StdapiPlugin + 1444u),
        StdapiProxyCfgAutodetect = MetaType.Bool | (StdapiPlugin + 1445u),
        StdapiProxyCfgAutoConfigUrL = MetaType.String | (StdapiPlugin + 1446u),
        StdapiProxyCfgProxy = MetaType.String | (StdapiPlugin + 1447u),
        StdapiProxyCfgProxyBypass = MetaType.String | (StdapiPlugin + 1448u),
        StdapiPeerHost = MetaType.String | (StdapiPlugin + 1500u),
        StdapiPeerPort = MetaType.Uint | (StdapiPlugin + 1501u),
        StdapiLocalHost = MetaType.String | (StdapiPlugin + 1502u),
        StdapiLocalPort  = MetaType.Uint | (StdapiPlugin + 1503u),
        StdapiConnectRetries = MetaType.Uint | (StdapiPlugin + 1504u),
        StdapiNetstatEntry = MetaType.Group | (StdapiPlugin + 1505u),
        StdapiPeerHostRaw = MetaType.Raw | (StdapiPlugin + 1506u),
        StdapiLocalHostRaw = MetaType.Raw | (StdapiPlugin + 1507u),
        StdapiShutdownHow = MetaType.Uint | (StdapiPlugin + 1530u),
        StdapiProcessId = MetaType.Uint | (StdapiPlugin + 2300u),
        StdapiProcessName = MetaType.String | (StdapiPlugin + 2301u),
        StdapiProcessPath = MetaType.String | (StdapiPlugin + 2302u),
        StdapiProcessGroup = MetaType.Group | (StdapiPlugin + 2303u),
        StdapiProcessFlags = MetaType.Uint | (StdapiPlugin + 2304u),
        StdapiProcessArguments = MetaType.String | (StdapiPlugin + 2305u),
        StdapiProcessArch = MetaType.Uint | (StdapiPlugin + 2306u),
        StdapiProcessParentProcessId = MetaType.Uint | (StdapiPlugin + 2307u),
        StdapiProcessSession = MetaType.Uint | (StdapiPlugin + 2308u),
        StdapiPowerFlags= MetaType.Uint | (StdapiPlugin + 4100u),
        StdapiPowerReason= MetaType.Uint | (StdapiPlugin + 4101u),
    }

    public class Tlv : ITlv
    {
        private object value = null;
        public TlvType Type { get; set; }
        public Dictionary<TlvType, List<Tlv>> Tlvs { get; private set; }

        private Tlv()
        {
            this.Tlvs = new Dictionary<TlvType, List<Tlv>>();
        }

        public Tlv(BinaryReader reader)
            : this()
        {
            var length = reader.ReadDword() - 8;
            this.Type = reader.ReadTlvType();
            var metaType = this.Type.ToMetaType();

            // Handle the process of sub-TLVs in the case of a group TLV
            if (metaType == MetaType.Group)
            {
                using (var stream = new MemoryStream(reader.ReadBytes((int)length)))
                using (var groupReader = new BinaryReader(stream))
                {
                    while (!groupReader.IsFinished())
                    {
                        Add(new Tlv(groupReader));
                    }
                }
            }
            // TODO: support compression
            //else if (metaType.HasFlag(MetaType.Compressed))
            //{
            //    int x = 0;
            //}
            else
            {
                // Otherwise, handle each value on its merit
                switch (metaType)
                {
                    case MetaType.Bool:
                        {
                            this.value = reader.ReadBoolean();
                            break;
                        }
                    case MetaType.Qword:
                        {
                            this.value = reader.ReadQword();
                            break;
                        }
                    case MetaType.Complex:
                    case MetaType.Raw:
                        {
                            this.value = reader.ReadBytes((int)length);
                            break;
                        }
                    case MetaType.String:
                        {
                            this.value = reader.ReadString((int)length);
                            break;
                        }
                    case MetaType.Uint:
                        {
                            this.value = reader.ReadDword();
                            break;
                        }
                    case MetaType.None:
                        {
                            throw new NotImplementedException(string.Format("Sorry, don't support {0} yet", metaType));
                        }
                    default:
                        {
                            throw new ArgumentException(string.Format("Unexpected MetaType {0}", metaType));
                        }
                }
            }
        }

        public void ToRaw(BinaryWriter writer)
        {
            var metaType = this.Type.ToMetaType();
            if (metaType == MetaType.Group)
            {
                var tlvData = default(byte[]);

                using (var tlvStream = new MemoryStream())
                using (var tlvWriter = new BinaryWriter(tlvStream))
                {
                    foreach (var tlv in this.Tlvs.Values.AsEnumerable().Flatten())
                    {
                        tlv.ToRaw(tlvWriter);
                    }
                    tlvData = tlvStream.ToArray();
                }

                writer.WriteDword((UInt32)tlvData.Length + 8u);
                writer.WriteTlvType(this.Type);
                writer.Write(tlvData);
            }
            else
            {
                switch (metaType)
                {
                    case MetaType.Bool:
                        {
                            writer.WriteDword(1u + 8u);
                            writer.WriteTlvType(this.Type);
                            writer.Write(this.ValueAsBool());
                            break;
                        }
                    case MetaType.Uint:
                        {
                            writer.WriteDword(4u + 8u);
                            writer.WriteTlvType(this.Type);
                            writer.WriteDword(this.ValueAsDword());
                            break;
                        }
                    case MetaType.Qword:
                        {
                            writer.WriteDword(8u + 8u);
                            writer.WriteTlvType(this.Type);
                            writer.WriteQword(this.ValueAsQword());
                            break;
                        }
                    case MetaType.String:
                        {
                            var val = this.ValueAsString();
                            writer.WriteDword((UInt32)val.Length + 1u + 8u);
                            writer.WriteTlvType(this.Type);
                            writer.WriteString(val);
                            break;
                        }
                    case MetaType.Raw:
                    case MetaType.Complex:
                        {
                            var val = this.ValueAsRaw();
                            writer.WriteDword((UInt32)val.Length + 8u);
                            writer.WriteTlvType(this.Type);
                            writer.Write(val);
                            break;
                        }
                    case MetaType.None:
                    case MetaType.Compressed:
                        {
                            throw new NotImplementedException(string.Format("Sorry, don't support {0} yet", metaType));
                        }
                    default:
                        {
                            throw new ArgumentException(string.Format("Unexpected MetaType {0}", metaType));
                        }
                }
            }
        }

        public Tlv(TlvType type, string value)
            : this()
        {
            this.Type = type;
            this.value = value;
            ValidateMetaType(MetaType.String);
        }

        public Tlv(TlvType type, Int32 value)
            : this(type, (UInt32)value)
        {
        }

        public Tlv(TlvType type, UInt32 value)
            : this()
        {
            this.Type = type;
            this.value = value;
            ValidateMetaType(MetaType.Uint);
        }

        public Tlv(TlvType type, Int64 value)
            : this(type, (UInt64)value)
        {
        }

        public Tlv(TlvType type, UInt64 value)
            : this()
        {
            this.Type = type;
            this.value = value;
            ValidateMetaType(MetaType.Qword);
        }

        public Tlv(TlvType type, byte[] value)
            : this()
        {
            this.Type = type;
            this.value = value;
            ValidateMetaType(MetaType.Raw, MetaType.Complex);
        }

        public Tlv(TlvType type, bool value)
            : this()
        {
            this.Type = type;
            this.value = value;
            ValidateMetaType(MetaType.Bool);
        }

        public Tlv(TlvType type)
            : this()
        {
            this.Type = type;
        }

        public Tlv Add(TlvType type, string value)
        {
            ValidateMetaType(MetaType.Group);
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, bool value)
        {
            ValidateMetaType(MetaType.Group);
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, byte[] value)
        {
            ValidateMetaType(MetaType.Group);
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, Int32 value)
        {
            ValidateMetaType(MetaType.Group);
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, Int64 value)
        {
            ValidateMetaType(MetaType.Group);
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, UInt32 value)
        {
            ValidateMetaType(MetaType.Group);
            return this.Add(new Tlv(type, value));
        }

        public Tlv Add(TlvType type, UInt64 value)
        {
            ValidateMetaType(MetaType.Group);
            return this.Add(new Tlv(type, value));
        }

        public Tlv AddGroup(TlvType type)
        {
            ValidateMetaType(MetaType.Group);
            return this.Add(new Tlv(type));
        }

        public Tlv Add<T>(TlvType type, T value) where T : struct
        {
            var meta = type.ToMetaType();
            if (meta != MetaType.Raw && meta != MetaType.Complex)
            {
                throw new ArgumentException(string.Format("Unable to serialise struct to type: {0}", meta));
            }

            return this.Add(new Tlv(type, value.ToByteArray()));
        }


        public Tlv Add(Tlv tlv)
        {
            ValidateMetaType(MetaType.Group);

            var tlvs = default(List<Tlv>);

            if (this.Tlvs.TryGetValue(tlv.Type, out tlvs))
            {
                tlvs.Add(tlv);
            }
            else
            {
                this.Tlvs.Add(tlv.Type, new List<Tlv> { tlv });
            }

            return tlv;
        }

        public string ValueAsString()
        {
            ValidateType<string>();
            return (string)this.value;
        }

        public bool ValueAsBool()
        {
            ValidateType<bool>();
            return (bool)this.value;
        }

        public UInt32 ValueAsDword()
        {
            ValidateType<UInt32>();
            return (UInt32)this.value;
        }

        public UInt64 ValueAsQword()
        {
            ValidateType<UInt64>();
            return (UInt64)this.value;
        }

        public byte[] ValueAsRaw()
        {
            ValidateType<byte[]>();
            return (byte[])this.value;
        }

#if DEBUG
        public override string ToString()
        {
            var s = new StringBuilder();
            s.AppendFormat("Tlv Type: {0}\n", this.Type);

            if (this.Type.ToMetaType() == MetaType.Group)
            {
                foreach (var value in this.Tlvs)
                {
                    s.AppendFormat(value.ToString());
                }
            }
            else
            {
                s.AppendFormat("{0}\n", this.value.ToString());
            }
            s.AppendLine();
            return s.ToString();
        }
#endif

        private void ValidateMetaType(params MetaType[] expectedTypes)
        {
            if (!expectedTypes.Contains(this.Type.ToMetaType()))
            {
                throw new InvalidOperationException(string.Format("Expecting MetaType {0} but provided type {1}", expectedTypes, this.Type));
            }
        }

        private void ValidateType<T>()
        {
            if (this.value == null)
            {
                throw new InvalidOperationException(string.Format("Unable to extract value from a TLV of type {0}", MetaType.Group));
            }

            var expectedType = typeof(T);
            var actualType = this.value.GetType();

            if (expectedType != actualType)
            {
                throw new InvalidOperationException(string.Format("Expected type {0} does not match requested type {1}", expectedType, actualType));
            }
        }
    }
}
