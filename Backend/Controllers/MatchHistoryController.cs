using Backend.Models;
using Database;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    public class MatchHistoryController : IDisposable {
        private IRepository _repository;

        public MatchHistoryController(IRepository repository) {
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<MatchHistory> Get() {
            return _repository.Lobbies.Include(l => l.Reputations).ThenInclude(r => r.Reputation).Include(l => l.Players).OrderBy(l => l.Joined).Select(l => new MatchHistory() {
                Id = l.Id,
                Joined = l.Joined.ToString("d"),
                Name = l.Name,
                Started = l.Started.HasValue,
                NegativeReputations = l.Reputations.Count(r => r.Reputation.Type == Database.Domain.ReputationType.NEGATIVE),
                PositiveReputations = l.Reputations.Count(r => r.Reputation.Type == Database.Domain.ReputationType.POSITIVE),
                Players = l.Players.Count(p => p.User != null && p.Position > 0)
            });
        }

        public void Dispose() {
            _repository.Dispose();
        }
    }
}
