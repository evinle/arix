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
            // await _db.GetPlayerCollection().Find(GetPlayerByIdFilter(id)).FirstOrDefaultAsync();
            await GetFirstPlayerByFields(new Dictionary<string, object>
            {
                [nameof(Player.Id)] = id,
            });

        public async Task<Player?> GetPlayerFromUsername(string username) =>
            //await _db.GetPlayerCollection().Find(GetPlayerByUsernameFilter(username)).FirstOrDefaultAsync();
            await GetFirstPlayerByFields(new Dictionary<string, object>
            {
                [nameof(Player.Username)] = username,
            });

        
        public FilterDefinition<Player> GetPlayerFilterByFields(Dictionary<string, object> fieldsAndSearchValues)
        {
            
            var filter = Builders<Player>.Filter.Empty;
            foreach (var searchKeyValue in fieldsAndSearchValues)
            {
                filter = filter
                    & Builders<Player>.Filter
                                // x[searchKeyValue.Key]
                    .Where(x => x.GetType().GetProperty(searchKeyValue.Key).GetValue(x) == searchKeyValue.Value);
            }
            return filter;
        }
        public async Task<List<Player>> GetPlayerByFields(Dictionary<string, object> fieldsAndSearchValues)
        {
            return _db.GetPlayerCollection().Find(GetPlayerFilterByFields(fieldsAndSearchValues)).ToList();
        }
        public async Task<Player?> GetFirstPlayerByFields(Dictionary<string, object> fieldsAndSearchValues)  => 
        (await GetPlayerByFields(fieldsAndSearchValues)).FirstOrDefault();


        public async Task CreatePlayer(Player newPlayer) =>
            await _db.GetPlayerCollection().InsertOneAsync(newPlayer);

        public async Task UpdatePlayer(string id, Player updatedplayer) =>
            await _db.GetPlayerCollection().ReplaceOneAsync(GetPlayerFilterByFields(new Dictionary<string, object>
            {[nameof(Player.Id)] = id}), updatedplayer);

        public async Task<DeleteResult> RemovePlayer(string id) =>
            await _db.GetPlayerCollection().DeleteOneAsync(GetPlayerFilterByFields(new Dictionary<string, object>
            {[nameof(Player.Id)] = id}));

        public async Task<Player?> GetLogin(string username)
        {
            //var filter = GetLoginFilter(username, password);
            return await GetFirstPlayerByFields(new Dictionary<string, object>
            {[nameof(Player.Username)] = username});
            //return player;
        }


        // private static MongoDB.Driver.FilterDefinition<Player> GetPlayerByIdFilter(string id)
        // {
        //     return Builders<Player>.Filter.Eq(player => player.Id, id);
        // }
        // private static MongoDB.Driver.FilterDefinition<Player> GetPlayerByUsernameFilter(string username)
        // {
        //     return Builders<Player>.Filter.Eq(player => player.Username, username);
        // }
        // private static MongoDB.Driver.FilterDefinition<Player> GetLoginFilter(string username, string password)
        // {
        //     var usernameFilter = Builders<Player>.Filter.Eq(player => player.Username, username);
        //     var passwordFilter = Builders<Player>.Filter.Eq(player => player.Password, password);
        //     return Builders<Player>.Filter.And(usernameFilter, passwordFilter);
        // }
    }
}