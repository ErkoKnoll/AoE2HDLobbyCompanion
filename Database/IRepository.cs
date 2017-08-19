using Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database {
    public interface IRepository : IDisposable {
        IQueryable<User> Users { get; }
        IQueryable<Lobby> Lobbies { get; }
        IQueryable<LobbySlot> LobbySlots { get; }
        IQueryable<UserReputation> UserReputations { get; }
        IQueryable<Reputation> Reputations { get; }
        IQueryable<KeyValuePair> KeyValuePairs { get; }

        void Add(User player);
        void Add(Lobby lobby);
        void Add(LobbySlot lobbySlot);
        void Add(UserReputation userReputation);
        void Add(Reputation reputation);
        void Add(KeyValuePair keyValuePair);

        void SetModified(Object obj);
        void Delete(UserReputation userReputation);
        void Delete(Reputation reputation);
        void Delete(LobbySlot reputation);
        void SaveChanges();

        void UpdateDatabase();
    }
}
