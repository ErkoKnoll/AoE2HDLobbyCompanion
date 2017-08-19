using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Domain {
    public class LobbySlot {
        public int Id { get; set; }
        public int Position { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int RankRM { get; set; }
        public int RankDM { get; set; }
        public int GamesStartedRM { get; set; }
        public int GamesEndedRM { get; set; }
        public int GamesWonRM { get; set; }
        public int GamesStartedDM { get; set; }
        public int GamesEndedDM { get; set; }
        public int GamesWonDM { get; set; }
        public User User { get; set; }
        public Lobby Lobby { get; set; }
        public List<UserReputation> Reputations { get; set; }
    }
}
