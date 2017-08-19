using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models {
    public class UserReputation {
        public int Id { get; set; }
        public string Comment { get; set; }
        public string Added { get; set; }
        public Reputation Reputation { get; set; }
        public Lobby Lobby { get; set; }
    }
}
