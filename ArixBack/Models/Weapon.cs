using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArixBack.Models
{
    public class Weapon
    {

        [BsonId]
        public int Id { get; set; }

        [BsonElement("weaponName")]
        [JsonPropertyName("weaponName")]
        public required string WeaponName { get; set; }

        public Weapon()
        {
        }
    }

    // public class WeaponWithOid: Weapon
    // {
    //     [BsonId]
    //     [BsonElement("_id")]
    //     public ObjectId? Id { get; set; }

    //     public WeaponWithOid(): base()
    //     {
    //     }
    // }
}