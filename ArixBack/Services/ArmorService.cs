using MongoDB.Driver;
using ArixBack.Models;

namespace ArixBack.Services
{
    public class ArmorService
    {
        private readonly IMongoCollection<Armor> _collection;

        public ArmorService(DatabaseService db) => _collection = db.GetArmorCollection();

        public async Task<List<Armor>> GetArmors() => await _collection.Find(_ => true).ToListAsync();

        public async Task<Armor?> GetArmor(string id) =>
            await _collection.Find(Builders<Armor>.Filter.Eq(a => a.Id, id)).FirstOrDefaultAsync();

        public async Task CreateArmor(Armor armor) => await _collection.InsertOneAsync(armor);
    }
}
