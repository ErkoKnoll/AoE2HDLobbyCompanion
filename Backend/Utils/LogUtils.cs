using Backend.Constants;
using Commons.Models;
using Commons.Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Utils {
    public class LogUtils {

        public static void Info(string message) {
            CommandUtils.QueueCommand(new LogCommand() {
                Id = (int)Commands.OUT.WRITE_LOG,
                Log = new Log {
                    Message = message,
                    Level = LogLevel.INFO
                }
            });
        }

        public static void Warn(string message) {
            CommandUtils.QueueCommand(new LogCommand() {
                Id = (int)Commands.OUT.WRITE_LOG,
                Log = new Log {
                    Message = message,
                    Level = LogLevel.WARN
                }
            });
        }

        public static void Error(string message) {
            CommandUtils.QueueCommand(new LogCommand() {
                Id = (int)Commands.OUT.WRITE_LOG,
                Log = new Log {
                    Message = message,
                    Level = LogLevel.ERROR
                }
            });
        }

        public static void Error(string message, Exception e) {
            CommandUtils.QueueCommand(new LogCommand() {
                Id = (int)Commands.OUT.WRITE_LOG,
                Log = new Log {
                    Message = message,
                    Payload = e.ToString(),
                    Level = LogLevel.ERROR
                }
            });
        }
    }
}
