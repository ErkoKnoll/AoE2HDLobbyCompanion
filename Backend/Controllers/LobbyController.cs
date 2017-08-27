using Backend.Global;
using Backend.Utils;
using Commons.Models;
using Database;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    public class LobbyController : Controller, IDisposable {
        private IRepository _repository;

        public LobbyController(IRepository repository) {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public Lobby Get(string id) {
            if (id == "0") {
                if (Variables.Lobby != null) {
                    lock (Variables.LobbyPlayers) {
                        LobbyUtils.CalculateLobbyPlayerFieldColors();
                        Variables.Lobby.Players = Variables.LobbyPlayers.OrderBy(lp => lp.Position);
                        Variables.Lobby.SLobbyId = Variables.Lobby.LobbyId.ToString();
                        foreach (var player in Variables.Lobby.Players) {
                            player.SSteamId = player.SteamId.ToString();
                        }
                        return Variables.Lobby;
                    }
                } else {
                    return null;
                }
            } else {
                var longLobbyId = ulong.Parse(id);
                var runningLobby = _repository.Lobbies.Include(l => l.Players).ThenInclude(ls => ls.User).FirstOrDefault(l => l.LobbyId == longLobbyId);
                var lobby = new Commons.Models.Lobby {
                    LobbyId = runningLobby.LobbyId,
                    SLobbyId = runningLobby.LobbyId.ToString(),
                    GameType = runningLobby.GameType,
                    Name = runningLobby.Name,
                    Ranked = runningLobby.Ranked,
                    Players = runningLobby.Players.Where(p => p.Position > 0).OrderBy(p => p.Position).Select(p => new Player {
                        Name = p.Name,
                        SteamId = p.User != null ? p.User.SteamId : 0,
                        SSteamId = p.User?.SteamId.ToString(),
                        LobbySlotId = p.Id,
                        Position = p.Position,
                        Rank = runningLobby.Ranked == 2 ? p.RankDM : p.RankRM,
                        RankRM = p.RankRM,
                        RankDM = p.RankDM,
                        Profile = p.User != null ? new PlayerProfile {
                            Location = p.User.Location,
                            ProfileDataFetched = p.User.ProfileDataFetched,
                            ProfilePrivate = p.User.ProfilePrivate
                        } : null,
                        ReputationStats = p.User != null ? new PlayerReputationStats {
                            Games = p.User.Games,
                            PositiveReputation = p.User.PositiveReputation,
                            NegativeReputation = p.User.NegativeReputation
                        } : null,
                        GameStats = p.User != null ? UserUtils.GetGameStats(p.GamesStartedRM, p.GamesStartedDM, p.GamesWonRM, p.GamesWonDM, p.GamesEndedRM, p.GamesEndedDM) : null,
                    }).ToList()
                };
                foreach (var player in lobby.Players) {
                    LobbyUtils.CalculateUserFieldColors(player, lobby.Ranked);
                }
                return lobby;
            }
        }
    }
}
