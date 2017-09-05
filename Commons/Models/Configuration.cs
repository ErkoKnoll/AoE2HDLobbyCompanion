using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Models {
    public class Configuration {
        public string ClientId { get; set; }
        public bool ShowOverlay { get; set; }
        public bool ShowOverlayWhenFocused { get; set; }
        public bool ShowOverlayWhenInLobby { get; set; }
        public string SteamApiKey { get; set; }
        public bool TosAccepted { get; set; }
        public bool SkipSessionHelp { get; set; }
        public string CmdPath { get; set; }
        public int OverlayActiveTopPosition { get; set; }
        public int OverlayActiveLeftPosition { get; set; }
        public int OverlayInactiveTopPosition { get; set; }
        public int OverlayInactiveLeftPosition { get; set; }
    }
}
