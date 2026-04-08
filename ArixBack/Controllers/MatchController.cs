using System.Collections.Generic;
using System.Threading.Tasks;
using ArixBack.Models;
using ArixBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ArixBack.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly DatabaseService _db;

        public MatchController(DatabaseService db)
        {
            _db = db;
        }

        [HttpGet("{id}/log")]
        public async Task<ActionResult<Match>> GetMatchLog(string id)
        {
            var filter = Builders<Match>.Filter.Eq(m => m.Id, id);
            var match = await _db.GetMatchCollection().Find(filter).FirstOrDefaultAsync();

            if (match == null)
            {
                return NotFound();
            }

            return Ok(match);
        }

        [HttpGet("history")]
        public async Task<ActionResult<List<Match>>> GetMatchHistory()
        {
            // Simple history for the current user could be implemented here
            // For now, return all matches (or a subset)
            var matches = await _db.GetMatchCollection().Find(_ => true).Limit(20).ToListAsync();
            return Ok(matches);
        }
    }
}
