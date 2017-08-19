using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commons.Models {
    public class Player : BasePlayer {
        public ulong SteamId { get; set; }
        public string SSteamId { get; set; } //JS 64 bit numbers may lose precision, so use string instead
        public int LobbySlotId { get; set; }
        public string Name { get; set; }
        public int Rank { get; set; }
        public int RankRM { get; set; }
        public int RankDM { get; set; }
        public int Position { get; set; }
    }
}
