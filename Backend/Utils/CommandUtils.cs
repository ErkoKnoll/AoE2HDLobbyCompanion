using Backend.Global;
using Commons.Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Utils {
    public class CommandUtils {
        public static void QueueCommand(BaseCommand command) {
            lock (Variables.OutgoingCommandsQueue) {
                Variables.OutgoingCommandsQueue.Add(command);
            }
        }
    }
}
