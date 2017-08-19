using ProtoBuf;
using SteamKit2;
using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Backend.Utils {
    class NetHookItem {
        public enum PacketDirection {
            In,
            Out
        }

        static Regex NameRegex = new Regex(@"(?<num>\d+)_(?<direction>in|out)_(?<emsg>\d+)_k_EMsg(?<name>[\w_<>]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Name { get; private set; }
        public int Sequence { get; private set; }
        public PacketDirection Direction { get; private set; }
        public EMsg EMsg { get; private set; }

        public string InnerMessageName {
            get { return innerMessageName ?? (innerMessageName = ReadInnerMessageName()); }
        }
        string innerMessageName;

        public FileInfo FileInfo { get; private set; }

        public Stream OpenStream() {
            return FileInfo.OpenRead();
        }

        public bool LoadFromFile(FileInfo fileInfo) {
            Match m = NameRegex.Match(fileInfo.Name);

            if (!m.Success) {
                return false;
            }

            if (!int.TryParse(m.Groups["num"].Value, out int sequence)) {
                return false;
            }

            var direction = m.Groups["direction"].Value;
            if (!Enum.TryParse<PacketDirection>(direction, ignoreCase: true, result: out PacketDirection packetDirection)) {
                return false;
            }

            if (!uint.TryParse(m.Groups["emsg"].Value, out uint emsg)) {
                return false;
            }

            FileInfo = fileInfo;

            Sequence = sequence;
            Direction = packetDirection;
            EMsg = (EMsg)emsg;
            Name = m.Groups["name"].Value;

            return true;
        }

        string ReadInnerMessageName() {
            try {
                return ReadInnerMessageNameCore();
            } catch (IOException) {
                return null;
            }
        }

        string ReadInnerMessageNameCore() {
            switch (EMsg) {
                case EMsg.ServiceMethod: {
                        var fileData = File.ReadAllBytes(FileInfo.FullName);
                        var hdr = new MsgHdrProtoBuf();
                        using (var ms = new MemoryStream(fileData)) {
                            hdr.Deserialize(ms);
                        }

                        return hdr.Proto.target_job_name;
                    }
                case EMsg.ClientServiceMethod: {
                        var proto = ReadAsProtobufMsg<CMsgClientServiceMethod>();
                        return proto.Body.method_name;
                    }
                case EMsg.ClientServiceMethodResponse: {
                        var proto = ReadAsProtobufMsg<CMsgClientServiceMethodResponse>();
                        return proto.Body.method_name;
                    }
                default:
                    return string.Empty;
            }
        }

        public ClientMsgProtobuf<T> ReadAsProtobufMsg<T>() where T : IExtensible, new() {
            var fileData = File.ReadAllBytes(FileInfo.FullName);
            var msg = new PacketClientMsgProtobuf(EMsg, fileData);
            var proto = new ClientMsgProtobuf<T>(msg);
            return proto;
        }
    }
}
