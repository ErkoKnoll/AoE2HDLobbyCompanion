using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Domain {
    public class Reputation {
        public int Id { get; set; }
        public string Name { get; set; }
        public ReputationType Type { get; set; }
        public bool CommentRequired { get; set; }
        public int OrderSequence { get; set; }
        public List<UserReputation> UserReputations { get; set; }
    }

    public enum ReputationType {
        NEGATIVE = 0,
        POSITIVE = 1
    }
}
