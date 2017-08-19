using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Models {
    public class BasePlayer {
        public Dictionary<string, int> FieldColors { get; set; }
        public PlayerGameStats GameStats { get; set; }
        public PlayerProfile Profile { get; set; }
        public PlayerReputationStats ReputationStats { get; set; }
    }
}
