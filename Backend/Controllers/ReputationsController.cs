using Database;
using Database.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    public class ReputationsController : IDisposable {
        private IRepository _repository;

        public ReputationsController(IRepository repository) {
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<Reputation> Get() {
            return _repository.Reputations;
        }

        public void Dispose() {
            _repository.Dispose();
        }
    }
}
