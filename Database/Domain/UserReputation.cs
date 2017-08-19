using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Domain {
    public class UserReputation {
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime Added { get; set; }
        public Reputation Reputation { get; set; }
        public Lobby Lobby { get; set; }
        public LobbySlot LobbySlot { get; set; }
        public User User { get; set; }
    }
}
