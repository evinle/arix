using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArixBack.Models
{
    public record MatchAction(DateTime Timestamp, string PlayerId, string ActionType, string? Payload);

    public class MatchLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("player1Id")]
        public string Player1Id { get; set; } = "";

        [BsonElement("player2Id")]
        public string Player2Id { get; set; } = "";

        [BsonElement("startedAt")]
        public DateTime StartedAt { get; set; }

        [BsonElement("endedAt")]
        public DateTime EndedAt { get; set; }

        [BsonElement("winnerId")]
        public string WinnerId { get; set; } = "";

        [BsonElement("actions")]
        public List<MatchAction> Actions { get; set; } = new();
    }
}
