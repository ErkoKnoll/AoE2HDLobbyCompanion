using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models {
    public class MatchHistoryLobbySlot {
        public string SSteamId { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public int Rank { get; set; }
        public int TotalGames { get; set; }
        public int WinRatio { get; set; }
        public int DropRatio { get; set; }
        public bool ProfilePrivate { get; set; }
    }
}
