using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArixBack.Models
{
    public class Armor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [BsonElement("damageReductionModifier")]
        [JsonPropertyName("damageReductionModifier")]
        public double DamageReductionModifier { get; set; } = 1.0;

        [BsonElement("specialEffect")]
        [JsonPropertyName("specialEffect")]
        public string? SpecialEffect { get; set; }
    }
}
