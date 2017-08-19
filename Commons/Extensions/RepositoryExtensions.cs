using Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Extensions {
    public static class RepositoryExtensions {
        public static IQueryable<LobbySlot> GetLobbySlot(this IQueryable<LobbySlot> lobbySlots, ulong userSteamId, ulong lobbyId) {
            return lobbySlots.Where(ls => ls.User.SteamId == userSteamId && ls.Lobby.LobbyId == lobbyId);
        }
    }
}
