using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArixBack.Models
{
    public class Match
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("startTime")]
        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }

        [BsonElement("endTime")]
        [JsonPropertyName("endTime")]
        public DateTime EndTime { get; set; }

        [BsonElement("players")]
        [JsonPropertyName("players")]
        public List<MatchPlayerInfo> Players { get; set; } = new List<MatchPlayerInfo>();

        [BsonElement("winnerId")]
        [JsonPropertyName("winnerId")]
        public string? WinnerId { get; set; }

        [BsonElement("eventLog")]
        [JsonPropertyName("eventLog")]
        public List<MatchEvent> EventLog { get; set; } = new List<MatchEvent>();
    }

    public class MatchPlayerInfo
    {
        [JsonPropertyName("playerId")]
        public string PlayerId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("eloBefore")]
        public int EloBefore { get; set; }

        [JsonPropertyName("eloAfter")]
        public int EloAfter { get; set; }
    }

    public class MatchEvent
    {
        [JsonPropertyName("ms")]
        public long Milliseconds { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        [JsonPropertyName("playerId")]
        public string? PlayerId { get; set; }
    }
}
