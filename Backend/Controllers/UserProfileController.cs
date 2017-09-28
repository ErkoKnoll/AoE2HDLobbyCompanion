using Database;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Commons.Models;
using Newtonsoft.Json;
using Backend.Models;
using Backend.Utils;
using Backend.Global;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    public class UserProfileController : IDisposable {
        private IRepository _repository;

        public UserProfileController(IRepository repository) {
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<Player> Get() {
            //Secondary where clause is done in memory because there seems to be an EF bug
            var users = _repository.Users.Where(u => u.Games > 0).ToList().Where(u => u.SteamId > 0).Select(u => new Player {
                Name = u.Name,
                RankRM = u.RankRM,
                RankDM = u.RankDM,
                SSteamId = u.SteamId.ToString(),
                GameStats = UserUtils.GetGameStats(u.GamesStartedRM, u.GamesStartedDM, u.GamesWonRM, u.GamesWonDM, u.GamesEndedRM, u.GamesEndedDM),
                Profile = new PlayerProfile {
                    Location = u.Location,
                    ProfilePrivate = u.ProfilePrivate,
                    ProfileDataFetched = u.ProfileDataFetched
                },
                ReputationStats = new PlayerReputationStats {
                    Games = u.Games,
                    NegativeReputation = u.NegativeReputation,
                    PositiveReputation = u.PositiveReputation
                }
            }).ToList();
            foreach (var user in users) {
                LobbyUtils.CalculateUserFieldColors(user, 0);
            }
            return users;
        }

        [HttpGet("{id}")]
        public User Get(string id) {
            var user = _repository.Users.Include(u => u.LobbySlots).ThenInclude(ls => ls.Lobby).Include(u => u.Reputations).ThenInclude(ur => ur.Reputation).Include(u => u.Reputations).ThenInclude(u => u.Lobby).Select(u => new User {
                Id = u.Id,
                SSteamId = u.SteamId.ToString(),
                Name = u.Name,
                Location = u.Location,
                Games = u.Games,
                PositiveReputation = u.PositiveReputation,
                NegativeReputation = u.NegativeReputation,
                RankRM = u.RankRM,
                RankDM = u.RankDM,
                GamesStartedRM = u.GamesStartedRM,
                GamesEndedRM = u.GamesEndedRM,
                GamesWonRM = u.GamesWonRM,
                GamesStartedDM = u.GamesStartedDM,
                GamesEndedDM = u.GamesEndedDM,
                GamesWonDM = u.GamesWonDM,
                ProfilePrivate = u.ProfilePrivate,
                ProfileDataFetched = u.ProfileDataFetched.HasValue ? u.ProfileDataFetched.Value.ToString("d") : null,
                KnownNames = u.LobbySlots.Select(ls => ls.Name).Distinct().OrderBy(ls => ls).ToList(),
                ReputationStats = new PlayerReputationStats() {
                    Games = u.Games,
                    PositiveReputation = u.PositiveReputation,
                    NegativeReputation = u.NegativeReputation
                },
                Reputations = u.Reputations.OrderByDescending(r => r.Added).Select(ur => new UserReputation {
                    Id = ur.Id,
                    Comment = ur.Comment,
                    Added = ur.Added.ToString("d"),
                    Reputation = new Reputation {
                        Id = ur.Reputation.Id,
                        Name = ur.Reputation.Name,
                        Type = ur.Reputation.Type,
                    },
                    Lobby = ur.Lobby != null ? new Models.Lobby {
                        Id = ur.Lobby.Id,
                        Name = ur.Lobby.Name
                    } : null
                }).ToList(),
                Matches = u.LobbySlots.Where(ls => ls.Lobby.Started.HasValue && ls.Position > 0).OrderByDescending(ls => ls.Lobby.Joined).Select(ls => new MatchHistory {
                    Id = ls.Lobby.Id,
                    Name = ls.Lobby.Name,
                    Joined = ls.Lobby.Joined.ToString("d")
                }).ToList()
            }).FirstOrDefault(u => u.SSteamId == id);
            user.GameStats = UserUtils.GetGameStats(user.GamesStartedRM, user.GamesStartedDM, user.GamesWonRM, user.GamesWonDM, user.GamesEndedRM, user.GamesEndedDM);
            LobbyUtils.CalculateUserFieldColors(user, 0);
            return user;
        }

        public void Dispose() {
            _repository.Dispose();
        }
    }
}
