using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArixBack.Models
{
    public enum ClassType { Rogue = 0, Berserker = 1, Juggernaut = 2, Wizard = 3 }

    public class Player
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [BsonElement("gold")]
        [JsonPropertyName("gold")]
        public int Gold { get; set; }

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [BsonElement("password")]
        [JsonIgnore]
        public string? Password { get; set; }

        [BsonElement("tag")]
        [JsonPropertyName("tag")]
        public LoginType LoginType { get; set; }

        [BsonElement("elo")]
        [JsonPropertyName("elo")]
        public int Elo { get; set; } = 1000;

        [BsonElement("classType")]
        [JsonPropertyName("classType")]
        public ClassType ClassType { get; set; } = ClassType.Rogue;

        [BsonElement("equippedWeaponId")]
        [JsonPropertyName("equippedWeaponId")]
        public string? EquippedWeaponId { get; set; }

        [BsonElement("equippedArmorId")]
        [JsonPropertyName("equippedArmorId")]
        public string? EquippedArmorId { get; set; }

        public Player(string username, string email, string password, LoginType loginType)
        {
            Username = username;
            Gold = 0;
            Email = email;
            Password = password;
            LoginType = loginType;
        }

        public Player(string username, string email, LoginType loginType)
        {
            Username = username;
            Gold = 0;
            Email = email;
            LoginType = loginType;
        }
    }
}
