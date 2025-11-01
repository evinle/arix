using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }


        [BsonElement("weaponName")]
        [JsonPropertyName("weaponName")]
        public required string WeaponName { get; set; }

        public Weapon()
        {
        }
    }


}