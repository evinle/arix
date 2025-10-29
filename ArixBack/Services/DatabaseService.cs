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

        public DatabaseService(IMongoDatabase database)
        {
            _weaponCollection = database.GetCollection<Weapon>("Weapons");
            _playerCollection = database.GetCollection<Player>("Player");
        }

        public IMongoCollection<Weapon> GetWeaponCollection()
        {
            return _weaponCollection;
        }
        public IMongoCollection<Player> GetPlayerCollection()
        {
            return _playerCollection;
        }
    }
}