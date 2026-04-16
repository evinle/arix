using MongoDB.Driver;
using ArixBack.Models;

namespace ArixBack.Services
{
    public class DatabaseService
    {
        private readonly IMongoCollection<Weapon> _weaponCollection;
        private readonly IMongoCollection<Player> _playerCollection;
        private readonly IMongoCollection<Armor> _armorCollection;
        private readonly IMongoCollection<MatchLog> _matchLogCollection;

        public DatabaseService(IMongoDatabase database)
        {
            _weaponCollection = database.GetCollection<Weapon>("Weapons");
            _playerCollection = database.GetCollection<Player>("Player");
            _armorCollection = database.GetCollection<Armor>("Armor");
            _matchLogCollection = database.GetCollection<MatchLog>("MatchLog");
        }

        public IMongoCollection<Weapon> GetWeaponCollection() => _weaponCollection;
        public IMongoCollection<Player> GetPlayerCollection() => _playerCollection;
        public IMongoCollection<Armor> GetArmorCollection() => _armorCollection;
        public IMongoCollection<MatchLog> GetMatchLogCollection() => _matchLogCollection;
    }
}
