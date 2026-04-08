using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ArixBack.Models;
using System.Runtime.CompilerServices;

namespace ArixBack.Services
{
    public class DatabaseService
    {
        private readonly IMongoCollection<Weapon> _weaponCollection;
        private readonly IMongoCollection<Player> _playerCollection;
        private readonly IMongoCollection<Match> _matchCollection;

        public DatabaseService(IMongoDatabase database)
        {
            _weaponCollection = database.GetCollection<Weapon>("Weapons");
            _playerCollection = database.GetCollection<Player>("Player");
            _matchCollection = database.GetCollection<Match>("Matches");
        }

        public IMongoCollection<Weapon> GetWeaponCollection()
        {
            return _weaponCollection;
        }
        public IMongoCollection<Player> GetPlayerCollection()
        {
            return _playerCollection;
        }
        public IMongoCollection<Match> GetMatchCollection()
        {
            return _matchCollection;
        }
    }
}