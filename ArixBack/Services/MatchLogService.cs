using ArixBack.Models;

namespace ArixBack.Services
{
    public class MatchLogService
    {
        private readonly DatabaseService _db;

        public MatchLogService(DatabaseService db) => _db = db;

        public async Task SaveLog(MatchLog log) =>
            await _db.GetMatchLogCollection().InsertOneAsync(log);
    }
}
