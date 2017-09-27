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
    public class ManageReputationsController : IDisposable {
        private IRepository _repository;

        public ManageReputationsController(IRepository repository) {
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<ReputationWithCount> Get() {
            return _repository.Reputations.Include(r => r.UserReputations).Select(r => new ReputationWithCount() {
                Id = r.Id,
                Name = r.Name,
                CommentRequired = r.CommentRequired,
                OrderSequence = r.OrderSequence,
                Type = r.Type,
                Total = r.UserReputations.Count()
            });
        }

        [HttpGet("{id}")]
        public IEnumerable<Models.UserReputation> Get(int id) {
            return _repository.Reputations.Include(r => r.UserReputations).ThenInclude(ur => ur.Lobby).Include(r => r.UserReputations).ThenInclude(ur => ur.User).FirstOrDefault(r => r.Id == id).UserReputations.OrderByDescending(ur => ur.Added).Select(ur => new Models.UserReputation() {
                Id = ur.Id,
                Added = ur.Added.ToString("d"),
                Comment = ur.Comment,
                User = new Models.User() {
                    SSteamId = ur.User.SteamId.ToString(),
                    Name = ur.User.Name
                },
                Lobby = new Models.Lobby() {
                    Id = ur.Lobby.Id,
                    LobbyId = ur.Lobby.LobbyId.ToString(),
                    Name = ur.Lobby.Name
                }
            });
        }

        [HttpPut]
        public void Put([FromBody] SaveReputationRequest request) {
            try {
                var nextOrderSequence = _repository.Reputations.Where(r => (int)r.Type == request.ReputationType).Max(r => r.OrderSequence) + 1;
                _repository.Add(new Database.Domain.Reputation() {
                    Name = request.Name,
                    Type = GetReputationType(request.ReputationType),
                    CommentRequired = request.CommentRequired,
                    OrderSequence = nextOrderSequence
                });
                _repository.SaveChanges();
            } catch (Exception e) {
                LogUtils.Error("Failed to add reputation", e);
                throw e;
            }
        }

        [HttpPost("{id}")]
        public void Post(int id, [FromBody] SaveReputationRequest request) {
            try {
                var reputation = _repository.Reputations.SingleOrDefault(r => r.Id == id);
                reputation.Name = request.Name;
                reputation.Type = GetReputationType(request.ReputationType);
                reputation.CommentRequired = request.CommentRequired;
                reputation.OrderSequence = request.OrderSequence;
                _repository.SetModified(reputation);
                _repository.SaveChanges();
            } catch (Exception e) {
                LogUtils.Error("Failed to save reputation", e);
                throw e;
            }
        }

        [HttpDelete("{id}")]
        public void Delete(int id, [FromQuery] int migrateTo) {
            try {
                var reputation = _repository.Reputations.Include(u => u.UserReputations).SingleOrDefault(r => r.Id == id);
                var users = _repository.Users.Include(u => u.Reputations).ThenInclude(ur => ur.Reputation).Where(u => u.Reputations.Any(r => r.Reputation.Id == id)).ToList();
                if (migrateTo == 0) {
                    foreach (var userReputation in reputation.UserReputations) {
                        _repository.Delete(userReputation);
                    }
                } else {
                    var migrateToReputation = _repository.Reputations.SingleOrDefault(r => r.Id == migrateTo);
                    if (migrateToReputation == null) {
                        throw new Exception("Migration reputation type not found: " + migrateTo);
                    }
                    foreach (var userReputation in new List<Database.Domain.UserReputation>(reputation.UserReputations)) {
                        userReputation.Reputation = migrateToReputation;
                        _repository.SetModified(userReputation);
                    }
                }
                _repository.Delete(reputation);
                _repository.SaveChanges();

                try {
                    foreach (var user in users) {
                        user.NegativeReputation = user.Reputations.Count(r => r.Reputation.Type == ReputationType.NEGATIVE);
                        user.PositiveReputation = user.Reputations.Count(r => r.Reputation.Type == ReputationType.POSITIVE);
                        _repository.SetModified(user);
                    }
                    _repository.SaveChanges();
                } catch (Exception e) {
                    LogUtils.Error("Failed to recalculate user reputations", e);
                }
                try {
                    var reputations = _repository.Reputations.Where(r => r.Type == reputation.Type).OrderBy(r => r.OrderSequence).ToList();
                    int index = 0;
                    foreach (var rep in reputations) {
                        rep.OrderSequence = index;
                        _repository.SetModified(rep);
                        index++;
                    }
                    _repository.SaveChanges();
                } catch (Exception e) {
                    LogUtils.Error("Failed to recalculate reputations sort order", e);
                }
            } catch (Exception e) {
                LogUtils.Error("Failed to delete reputation", e);
                throw e;
            }
        }

        private ReputationType GetReputationType(int id) {
            switch (id) {
                case 0:
                    return ReputationType.NEGATIVE;
                case 1:
                    return ReputationType.POSITIVE;
                default:
                    throw new Exception("Reputation type not found: " + id);
            }
        }

        public void Dispose() {
            _repository.Dispose();
        }
    }
}