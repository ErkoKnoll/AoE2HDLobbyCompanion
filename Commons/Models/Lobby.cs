using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Models {
    public class Lobby {
        public ulong LobbyId { get; set; }
        public string SLobbyId { get; set; } //JS 64 bit numbers may lose precision, so use string instead
        public string Name { get; set; }
        public int GameType { get; set; }
        public int Ranked { get; set; }
        public IEnumerable<Player> Players { get; set; }
    }
}
