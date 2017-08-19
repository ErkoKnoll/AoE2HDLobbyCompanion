using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Database.Domain {
    public class User {
        public int Id { get; set; }
        public ulong SteamId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int Games { get; set; }
        public int RankRM { get; set; }
        public int RankDM { get; set; }
        public int PositiveReputation { get; set; }
        public int NegativeReputation { get; set; }
        public int GamesStartedRM { get; set; }
        public int GamesEndedRM { get; set; }
        public int GamesWonRM { get; set; }
        public int GamesStartedDM { get; set; }
        public int GamesEndedDM { get; set; }
        public int GamesWonDM { get; set; }
        public bool ProfilePrivate { get; set; }
        public DateTime? ProfileDataFetched { get; set; }
        public List<LobbySlot> LobbySlots { get; set; }
        public List<UserReputation> Reputations { get; set; }
    }
}
