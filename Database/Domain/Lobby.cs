using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Domain {
    public class Lobby {
        public int Id { get; set; }
        public ulong LobbyId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int CheatsEnabled { get; set; }
        public int DataSet { get; set; }
        public int EndAge { get; set; }
        public int GameType { get; set; }
        public int GameVersion { get; set; }
        public int LatencyRegion { get; set; }
        public int LobbyElo { get; set; }
        public int MapSize { get; set; }
        public int MapStyleType { get; set; }
        public int MapType { get; set; }
        public int Pop { get; set; }
        public int Ranked { get; set; }
        public int Resource { get; set; }
        public int SlotsFilled { get; set; }
        public int Speed { get; set; }
        public int Victory { get; set; }
        public int PositiveReputations { get; set; }
        public int NegativeReputations { get; set; }
        public bool Sealed { get; set; }
        public DateTime Joined { get; set; }
        public DateTime? Started { get; set; }
        public List<LobbySlot> Players { get; set; }
        public List<UserReputation> Reputations { get; set; }
    }
}
