using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commons.Models {
    public class Version {
        public string StringVersion { get; set; }
        public int NumericVersion { get; set; }
        public string DownloadUrl { get; set; }
    }
}
