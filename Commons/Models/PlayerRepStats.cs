using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Models {
    public class PlayerReputationStats {
        public int Games { get; set; }
        public int NegativeReputation { get; set; }
        public int PositiveReputation { get; set; }
    }
}
