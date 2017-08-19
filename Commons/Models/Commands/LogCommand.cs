using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Models.Commands {
    public class LogCommand : BaseCommand {
        public Log Log { get; set; }
    }
}
