
using Commons.Models;
using Commons.Models.Commands;
using Overlay;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Backend.Global {
    public class Variables {
        public static int NumericVersion = 4;
        public static bool ReplayMode = true;
        public static bool LobbySession = false;
        public static string NethookDumpDir { get; set; }
        public static string NethookDumpDirParsed { get; set; }
        public static Configuration Config { get; set; }
        public static Thread OverlayThread { get; set; }
        public static Application OverlayApp { get; set; }
        public static MainWindow OverlayWindow { get; set; }
        public static Lobby Lobby { get; set; }
        public static List<Player> LobbyMembers = new List<Player>();
        public static List<Player> LobbyPlayers = new List<Player>();
        public static Dictionary<ulong, PlayerGameStats> PlayerGameStatsCache = new Dictionary<ulong, PlayerGameStats>();
        public static Dictionary<ulong, PlayerProfile> PlayerProfilesCache = new Dictionary<ulong, PlayerProfile>();
        public static Dictionary<ulong, PlayerReputationStats> PlayerReputationStatsCache = new Dictionary<ulong, PlayerReputationStats>();
        public static List<BaseCommand> OutgoingCommandsQueue = new List<BaseCommand>();
    }
}
