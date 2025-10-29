using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArixBack.Models
{
    public class Player
    {
        public Player(int id,string username, int gold)
        {
            this.id = id;
            this.username = username;
            this.gold = gold;
        }
        [BsonId]
        public int id { get; set; }
        public string username { get; set; }
        public int gold { get; set; }
    }
}