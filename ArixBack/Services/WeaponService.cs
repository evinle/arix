using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ArixBack.Models;

namespace ArixBack.Services
{
    public class WeaponService
    {
        
        private DatabaseService _db;
        public WeaponService(DatabaseService db)
        {
            _db = db;
        }
         public async Task<List<Weapon>> GetWeapons() =>
            await _db.GetWeaponCollection().Find(_ => true).ToListAsync();

        public async Task<Weapon?> GetWeapon(int id) =>
            await _db.GetWeaponCollection().Find(x => x.weaponId == id).FirstOrDefaultAsync();

        public async Task CreateWeapon(Weapon newWeapon) =>
            await _db.GetWeaponCollection().InsertOneAsync(newWeapon);

        public async Task UpdateWeapon(int id, Weapon updatedWeapon) =>
            await _db.GetWeaponCollection().ReplaceOneAsync(x => x.weaponId == id, updatedWeapon);

        public async Task RemoveWeapon(int id) =>
            await _db.GetWeaponCollection().DeleteOneAsync(x => x.weaponId == id);
        
    }
}