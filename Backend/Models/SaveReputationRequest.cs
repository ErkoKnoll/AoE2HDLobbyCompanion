using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models {
    public class SaveReputationRequest {
        public string Name { get; set; }
        public int ReputationType { get; set; }
        public bool CommentRequired { get; set; }
        public int OrderSequence { get; set; }
    }
}
