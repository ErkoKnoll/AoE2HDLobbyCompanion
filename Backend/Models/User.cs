using Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models {
    public class User : BasePlayer {
        public int Id { get; set; }
        public string SSteamId { get; set; }
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
        public string ProfileDataFetched { get; set; }
        public List<string> KnownNames { get; set; }
        public List<UserReputation> Reputations { get; set; }
        public List<MatchHistory> Matches { get; set; }
    }
}
