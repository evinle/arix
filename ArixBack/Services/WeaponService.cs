using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ArixBack.Models;
using MongoDB.Bson;

namespace ArixBack.Services
{
    public class WeaponService(DatabaseService db)
    {
        
        private DatabaseService _db => db;
        private IMongoCollection<Weapon> _weaponsCollection => db.GetWeaponCollection();
         public async Task<List<Weapon>> GetWeapons() =>
            await _weaponsCollection.Find(_ => true).ToListAsync();

        public async Task<Weapon?> GetWeapon(int id) =>
            await _weaponsCollection.Find(GetWeaponByIdFilter(id)).FirstOrDefaultAsync();
        

        public async Task CreateWeapon(Weapon newWeapon) =>
            await _db.GetWeaponCollection().InsertOneAsync(newWeapon);

        public async Task<bool> UpdateWeapon(int id, Weapon updatedWeapon)
        {
            var res = await _weaponsCollection.ReplaceOneAsync(GetWeaponByIdFilter(id), updatedWeapon);
            return res.ModifiedCount > 0;
        }

        public async Task<DeleteResult> RemoveWeapon(int id) =>
            await _weaponsCollection.DeleteOneAsync(GetWeaponByIdFilter(id));
        
        private static MongoDB.Driver.FilterDefinition<ArixBack.Models.Weapon> GetWeaponByIdFilter(int id)
        {
            return Builders<Weapon>.Filter.Eq(wep => wep.Id, id);  
        }
    }
}