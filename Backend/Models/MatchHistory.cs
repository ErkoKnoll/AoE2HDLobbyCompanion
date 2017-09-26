using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models {
    public class MatchHistory {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Players { get; set; }
        public int NegativeReputations { get; set; }
        public int PositiveReputations { get; set; }
        public string Joined { get; set; }
        public bool Started { get; set; }
        public List<MatchHistoryLobbySlot> LobbySlots { get; set; }
        public List<UserReputation> Reputations { get; set; }
    }
}
