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