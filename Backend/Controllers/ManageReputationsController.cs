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

        public void Dispose() {
            _repository.Dispose();
        }
    }
}