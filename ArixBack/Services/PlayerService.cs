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

        public async Task<Player?> GetPlayer(int id) =>
            await _db.GetPlayerCollection().Find(x => x.id == id).FirstOrDefaultAsync();

        public async Task CreatePlayer(Player newPlayer) =>
            await _db.GetPlayerCollection().InsertOneAsync(newPlayer);

        public async Task UpdatePlayer(int id, Player updatedplayer) =>
            await _db.GetPlayerCollection().ReplaceOneAsync(x => x.id == id, updatedplayer);

        public async Task RemovePlayer(int id) =>
            await _db.GetPlayerCollection().DeleteOneAsync(x => x.id == id);
    }
}