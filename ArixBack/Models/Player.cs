using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ArixBack.Models
{
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
      [JsonPropertyName("password")]
      public string Password { get; set; }
      public Player(string username, string email, string password)
      {
        Username = username;
        Gold = 0;
        Email = email;
        Password = password;
      }
    }
}