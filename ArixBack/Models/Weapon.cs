using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArixBack.Models
{
    public class Weapon
    {
        [BsonId]
        public int weaponId { get; set; }
        
        [BsonElement("weaponName")]
        public string weaponName { get; set; }

        public Weapon() {}
        public Weapon(int id, string name)
        {
            weaponId = id;
            weaponName = name;
        }
    }
}