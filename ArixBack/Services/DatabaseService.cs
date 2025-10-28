using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ArixBack.Models;

namespace ArixBack.Services
{
    public class DatabaseService
    {
        private readonly IMongoCollection<Weapon> _WeaponCollection;

        public DatabaseService(IMongoDatabase database)
        {
            _WeaponCollection = database.GetCollection<Weapon>("Weapons");
        }

        public async Task<List<Weapon>> GetAsync() =>
            await _WeaponCollection.Find(_ => true).ToListAsync();

        public async Task<Weapon?> GetAsync(int id) =>
            await _WeaponCollection.Find(x => x.weaponId == id).FirstOrDefaultAsync();

        public async Task CreateWeapon(Weapon newWeapon) =>
            await _WeaponCollection.InsertOneAsync(newWeapon);

        public async Task UpdateAsync(int id, Weapon updatedBook) =>
            await _WeaponCollection.ReplaceOneAsync(x => x.weaponId == id, updatedBook);

        public async Task RemoveAsync(int id) =>
            await _WeaponCollection.DeleteOneAsync(x => x.weaponId == id);
        }
}