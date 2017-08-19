using Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Reputation {
        public int Id { get; set; }
        public string Name { get; set; }
        public ReputationType Type { get; set; }
    }
}
