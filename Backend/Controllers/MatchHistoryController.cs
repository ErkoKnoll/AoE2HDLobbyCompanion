using Backend.Models;
using Database;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Database.Domain;
using Backend.Utils;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    public class MatchHistoryController : IDisposable {
        private IRepository _repository;

        public MatchHistoryController(IRepository repository) {
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<MatchHistory> Get() {
            return _repository.Lobbies.Include(l => l.Reputations).ThenInclude(r => r.Reputation).Include(l => l.Players).OrderByDescending(l => l.Joined).Select(l => new MatchHistory() {
                Id = l.Id,
                Joined = l.Joined.ToString("d"),
                Name = l.Name,
                Started = l.Started.HasValue,
                NegativeReputations = l.Reputations.Count(r => r.Reputation.Type == ReputationType.NEGATIVE),
                PositiveReputations = l.Reputations.Count(r => r.Reputation.Type == ReputationType.POSITIVE),
                Players = l.Players.Count(p => p.User != null && p.Position > 0)
            });
        }

        [HttpGet("{id}")]
        public MatchHistory Get(int id) {
            var lobby = _repository.Lobbies.Include(l => l.Reputations).ThenInclude(r => r.Reputation).Include(l => l.Reputations).ThenInclude(r => r.User).Include(l => l.Players).ThenInclude(p => p.User).SingleOrDefault(l => l.Id == id);
            var matchHistory = new MatchHistory {
                Id = lobby.Id,
                Joined = lobby.Joined.ToString("d"),
                Name = lobby.Name,
                Started = lobby.Started.HasValue,
                NegativeReputations = lobby.Reputations.Count(r => r.Reputation.Type == ReputationType.NEGATIVE),
                PositiveReputations = lobby.Reputations.Count(r => r.Reputation.Type == ReputationType.POSITIVE),
                Players = lobby.Players.Count(p => p.User != null && p.Position > 0),
                LobbySlots = new List<MatchHistoryLobbySlot>(),
                Reputations = lobby.Reputations.OrderBy(r => r.Reputation.Type).Select(r => new Models.UserReputation {
                    Id = r.Id,
                    Added = r.Added.ToString("d"),
                    Comment = r.Comment,
                    User = new Models.User {
                        Name = r.User.Name,
                        SSteamId = r.User.SteamId.ToString()
                    },
                    Reputation = new Models.Reputation {
                        Id = r.Reputation.Id,
                        Name = r.Reputation.Name,
                        Type = r.Reputation.Type
                    }
                }).ToList()
            };
            foreach (var lobbySlot in lobby.Players.Where(p => p.Position > 0)) {
                matchHistory.LobbySlots.Add(GetPlayer(lobbySlot));
            }
            matchHistory.LobbySlots = matchHistory.LobbySlots.Where(ls => ls.Position > 0).OrderBy(ls => ls.Position).ToList();
            return matchHistory;
        }

        private MatchHistoryLobbySlot GetPlayer(LobbySlot lobbySlot) {
            var player = new MatchHistoryLobbySlot {
                Name = lobbySlot.Name,
                SSteamId = lobbySlot.User?.SteamId.ToString(),
                Position = lobbySlot.Position,
                ProfilePrivate = lobbySlot.User != null ? lobbySlot.User.ProfilePrivate: false
            };
            if (lobbySlot.User != null) {
                if (lobbySlot.Lobby.Ranked == 2) {
                    player.Rank = lobbySlot.RankDM;
                    player.TotalGames = lobbySlot.GamesStartedDM;
                    player.WinRatio = lobbySlot.GamesStartedDM > 0 ? lobbySlot.GamesWonDM * 100 / lobbySlot.GamesStartedDM : 0;
                    player.DropRatio = lobbySlot.GamesStartedDM > 0 ? (lobbySlot.GamesStartedDM - lobbySlot.GamesEndedDM) * 100 / lobbySlot.GamesStartedDM : 0;
                } else {
                    player.Rank = lobbySlot.RankRM;
                    player.TotalGames = lobbySlot.GamesStartedRM;
                    player.WinRatio = lobbySlot.GamesStartedRM > 0 ? lobbySlot.GamesWonRM * 100 / lobbySlot.GamesStartedRM : 0;
                    player.DropRatio = lobbySlot.GamesStartedRM > 0 ? (lobbySlot.GamesStartedRM - lobbySlot.GamesEndedRM) * 100 / lobbySlot.GamesStartedRM : 0;
                }
                player.GameStats = UserUtils.GetGameStats(lobbySlot.GamesStartedRM, lobbySlot.GamesStartedDM, lobbySlot.GamesWonRM, lobbySlot.GamesWonDM, lobbySlot.GamesEndedRM, lobbySlot.GamesEndedDM);
                LobbyUtils.CalculateUserFieldColors(player, 0);
            }
            return player;
        }

        public void Dispose() {
            _repository.Dispose();
        }
    }
}
