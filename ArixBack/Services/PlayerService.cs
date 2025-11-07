using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ArixBack.Models;
using Microsoft.VisualBasic;
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

        public async Task<Player?> GetPlayerFromId(string id) =>
            await _db.GetPlayerCollection().Find(GetPlayerByIdFilter(id)).FirstOrDefaultAsync();
        public async Task<Player?> GetPlayerFromUsername(string username) =>
            await _db.GetPlayerCollection().Find(GetPlayerByUsernameFilter(username)).FirstOrDefaultAsync();

        public async Task CreatePlayer(Player newPlayer) =>
            await _db.GetPlayerCollection().InsertOneAsync(newPlayer);

        public async Task UpdatePlayer(string id, Player updatedplayer) =>
            await _db.GetPlayerCollection().ReplaceOneAsync(GetPlayerByIdFilter(id), updatedplayer);

        public async Task<DeleteResult> RemovePlayer(string id) =>
            await _db.GetPlayerCollection().DeleteOneAsync(GetPlayerByIdFilter(id));
    
        public async Task<Player?> GetLogin(string username,string password)
        {
            var filter = GetLoginFilter(username, password);
            var player = await _db.GetPlayerCollection().Find(filter).FirstOrDefaultAsync();
            return player;
        }
            

        private static MongoDB.Driver.FilterDefinition<Player> GetPlayerByIdFilter(string id)
        {
            return Builders<Player>.Filter.Eq(player => player.Id, id);  
        }
        private static MongoDB.Driver.FilterDefinition<Player> GetPlayerByUsernameFilter(string username)
        {
            return Builders<Player>.Filter.Eq(player => player.Username, username);  
        }
        private static MongoDB.Driver.FilterDefinition<Player> GetLoginFilter(string username,string password)
        {
            var usernameFilter = Builders<Player>.Filter.Eq(player => player.Username, username);
            var passwordFilter = Builders<Player>.Filter.Eq(player => player.Password, password);
            return Builders<Player>.Filter.And(usernameFilter, passwordFilter);
        }
    }
}