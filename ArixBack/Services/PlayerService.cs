using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ArixBack.Models;
namespace ArixBack.Services
{
    public class PlayerService
    {
        private DatabaseService _db;
        public PlayerService(DatabaseService db)
        {
            _db = db;
        }
         public async Task<List<Player>> GetPlayers() =>
            await _db.GetPlayerCollection().Find(_ => true).ToListAsync();

        public async Task<Player?> GetPlayer(string id) =>
            await _db.GetPlayerCollection().Find(GetPlayerByIdFilter(id)).FirstOrDefaultAsync();

        public async Task CreatePlayer(Player newPlayer) =>
            await _db.GetPlayerCollection().InsertOneAsync(newPlayer);

        public async Task UpdatePlayer(string id, Player updatedplayer) =>
            await _db.GetPlayerCollection().ReplaceOneAsync(GetPlayerByIdFilter(id), updatedplayer);

        public async Task<DeleteResult> RemovePlayer(string id) =>
            await _db.GetPlayerCollection().DeleteOneAsync(GetPlayerByIdFilter(id));

            private static MongoDB.Driver.FilterDefinition<Player> GetPlayerByIdFilter(string id)
        {
            return Builders<Player>.Filter.Eq(player => player.Id, id);  
        }
    }
}