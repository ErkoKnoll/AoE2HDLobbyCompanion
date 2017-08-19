using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Models.Commands {
    public class UpdateAvailableCommand : BaseCommand {
        public Models.Version Version { get; set; }
    }
}
