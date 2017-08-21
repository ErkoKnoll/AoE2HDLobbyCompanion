using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models {
    public class ReputationWithCount : Database.Domain.Reputation {
        public int Total { get; set; }
    }
}
