using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Models {
    public class Log {
        public string Message { get; set; }
        public object Payload { get; set; }
        public LogLevel Level { get; set; }
    }

    public enum LogLevel {
        INFO = 0,
        WARN = 1,
        ERROR = 2
    }
}
