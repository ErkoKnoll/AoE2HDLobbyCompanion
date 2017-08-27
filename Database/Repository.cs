using Database.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Database {
    public class Repository : DbContext, IRepository {
        public DbSet<User> Users { get; set; }
        public DbSet<Lobby> Lobbies { get; set; }
        public DbSet<LobbySlot> LobbySlots { get; set; }
        public DbSet<UserReputation> UserReputations { get; set; }
        public DbSet<Reputation> Reputations { get; set; }
        public DbSet<KeyValuePair> KeyValuePairs { get; set; }

        IQueryable<User> IRepository.Users => Users;
        IQueryable<Lobby> IRepository.Lobbies => Lobbies;
        IQueryable<LobbySlot> IRepository.LobbySlots => LobbySlots;
        IQueryable<UserReputation> IRepository.UserReputations => UserReputations;
        IQueryable<Reputation> IRepository.Reputations => Reputations;
        IQueryable<KeyValuePair> IRepository.KeyValuePairs => KeyValuePairs;

        public void Add(User player) {
            Users.Add(player);
        }

        public void Add(Lobby lobby) {
            Lobbies.Add(lobby);
        }

        public void Add(LobbySlot lobbySlot) {
            LobbySlots.Add(lobbySlot);
        }

        public void Add(UserReputation userReputation) {
            UserReputations.Add(userReputation);
        }

        public void Add(Reputation reputation) {
            Reputations.Add(reputation);
        }

        public void Add(KeyValuePair keyValuePair) {
            KeyValuePairs.Add(keyValuePair);
        }

        public void SetModified(Object obj) {
            Entry(obj).State = EntityState.Modified;
        }

        public void Delete(UserReputation userReputation) {
            Entry(userReputation).State = EntityState.Deleted;
        }

        public void Delete(Reputation reputation) {
            Entry(reputation).State = EntityState.Deleted;
        }

        public void Delete(LobbySlot lobbySlot) {
            Entry(lobbySlot).State = EntityState.Deleted;
        }

        public void UpdateDatabase() {
            Database.Migrate();
        }

        void IRepository.SaveChanges() {
            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            var databaseName = "AoE2HDLobbyCompanion\\database.db";
#if DEBUG
            databaseName = "AoE2HDLobbyCompanion\\database_test.db";
#endif
            var path = Path.Combine(Environment.GetEnvironmentVariable("AppData"), databaseName);
            optionsBuilder.UseSqlite("Data Source=" + path);
        }
    }
}
