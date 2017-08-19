using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Constants {
    public static class Commands {
        public enum IN {
            LOBBY_SESSION_START = 0,
            LOBBY_SESSION_STOP = 1,
            SET_UP = 2,
            START_LOBBY_MANUALLY = 3,
            SEAL_LOBBY = 4,
            GET_UNSEALED_LOBBY = 5,
            CALCULATE_BALANCED_TEAMS_BASED_ON_RANK = 6,
            CALCULATE_BALANCED_TEAMS_BASED_ON_TOTAL_GAMES = 7,
            CALCULATE_BALANCED_TEAMS_BASED_ON_WIN_RATIO = 8,
            COPY_PLAYER_STATS = 9
        }
        public enum OUT {
            GAME_STARTED = 0,
            WRITE_LOG = 1,
            UPDATE_AVAILABLE = 2
        }
    }
}
