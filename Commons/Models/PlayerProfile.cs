using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Models {
    public class PlayerProfile {
        public bool ProfilePrivate { get; set; }
        public string Location { get; set; }
        public DateTime? ProfileDataFetched { get; set; }
    }
}
