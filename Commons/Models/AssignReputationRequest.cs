using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Models {
    public class AssignReputationRequest {
        public string PlayerSteamId { get; set; }
        public string LobbyId { get; set; }
        public int LobbySlotId { get; set; }
        public int ReputationId { get; set; }
        public string Comment { get; set; }
    }
}
