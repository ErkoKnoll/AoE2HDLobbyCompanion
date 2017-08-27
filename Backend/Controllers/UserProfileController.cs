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

        [HttpGet("{id}")]
        public User Get(string id) {
            var user = _repository.Users.Include(u => u.LobbySlots).Include(u => u.Reputations).ThenInclude(ur => ur.Reputation).Include(u => u.Reputations).ThenInclude(u => u.Lobby).Select(u => new User {
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
