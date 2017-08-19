using Backend.Global;
using Backend.Utils;
using Commons.Models;
using Database;
using Database.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    public class UserReputationsController : IDisposable {
        private IRepository _repository;

        public UserReputationsController(IRepository repository) {
            _repository = repository;
        }

        [HttpPut]
        public void Put([FromBody] AssignReputationRequest request) {
            try {
                var user = _repository.Users.ToList().FirstOrDefault(u => u.SteamId == ulong.Parse(request.PlayerSteamId));
                var lobby = _repository.Lobbies.FirstOrDefault(u => u.LobbyId == ulong.Parse(request.LobbyId));
                var lobbySlot = _repository.LobbySlots.FirstOrDefault(ls => ls.Id == request.LobbySlotId);
                var reputation = _repository.Reputations.FirstOrDefault(r => r.Id == request.ReputationId);
                if (user == null) {
                    LogUtils.Error("Failed to assign reputation. User not found: " + request.PlayerSteamId);
                    throw new Exception("Failed to assign reputation. User not found: " + request.PlayerSteamId);
                }
                if (lobby == null) {
                    LogUtils.Error("Failed to assign reputation. Lobby not found: " + request.LobbyId);
                    throw new Exception("Failed to assign reputation. Lobby not found: " + request.LobbyId);
                }
                if (lobbySlot == null) {
                    LogUtils.Error("Failed to assign reputation. Lobby slot not found: " + request.LobbySlotId);
                    throw new Exception("Failed to assign reputation. Lobby slot not found: " + request.LobbySlotId);
                }
                if (reputation == null) {
                    LogUtils.Error("Failed to assign reputation. Reputation type not found: " + request.ReputationId);
                    throw new Exception("Failed to assign reputation. Reputation type not found: " + request.ReputationId);
                }
                _repository.Add(new UserReputation {
                    User = user,
                    Lobby = lobby,
                    LobbySlot = lobbySlot,
                    Reputation = reputation,
                    Comment = request.Comment,
                    Added = DateTime.UtcNow
                });
                if (reputation.Type == ReputationType.NEGATIVE) {
                    user.NegativeReputation++;
                } else {
                    user.PositiveReputation++;
                }
                _repository.SetModified(user);
                _repository.SaveChanges();
                if (Variables.Lobby != null) {
                    var player = Variables.Lobby.Players.FirstOrDefault(p => p.SteamId == user.SteamId);
                    if (player != null) {
                        UserUtils.FetchUserReputationStats(player, true);
                    }
                }
            } catch (Exception e) {
                LogUtils.Error("Failed to assign reputation", e);
                throw e;
            }
        }

        [HttpDelete("{id}")]
        public void Delete(int id) {
            try {
                var reputation = _repository.UserReputations.Include(ur => ur.User).Include(ur => ur.Reputation).SingleOrDefault(ur => ur.Id == id);
                if (reputation == null) {
                    throw new Exception("Reputation was not found in database");
                }
                if (reputation.Reputation.Type == ReputationType.NEGATIVE) {
                    reputation.User.NegativeReputation--;
                } else {
                    reputation.User.PositiveReputation--;
                }
                _repository.SetModified(reputation.User);
                _repository.Delete(reputation);
                _repository.SaveChanges();
                if (Variables.Lobby != null) {
                    var player = Variables.Lobby.Players.FirstOrDefault(p => p.SteamId == reputation.User.SteamId);
                    if (player != null) {
                        UserUtils.FetchUserReputationStats(player, true);
                    }
                }
            } catch (Exception e) {
                LogUtils.Error("Failed to delete reputation", e);
                throw e;
            }
        }

        public void Dispose() {
            _repository.Dispose();
        }
    }
}
