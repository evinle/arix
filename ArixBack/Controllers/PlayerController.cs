using Microsoft.AspNetCore.Mvc;
using ArixBack.Models;
using System.Diagnostics;
using System.Data.Common;
using ArixBack.Services;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
namespace ArixBack.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/players")]
    public class PlayerController : ControllerBase
    {

        private PlayerService _db;

        public PlayerController(PlayerService db)
        {
            _db = db;
        }
        [HttpGet("GetAllPlayers")]
        public async Task<ActionResult<List<Player>>> GetAllPlayers()
        {

            return Ok(await _db.GetPlayers());
        }

        [HttpGet("GetPlayer")]
        public async Task<ActionResult<Player>> GetPlayer(string id)
        {
            Player player = await _db.GetPlayer(id);
            if(player == null) return NotFound($"No player with ID: {id}");
            return Ok(player);
        }
        [HttpPost("CreatePlayer")]
        public async Task<ActionResult> CreatePlayer(Player player)
        {
            player.Id = null;
            await _db.CreatePlayer(player);
            return Ok();
        }
        [HttpPost("UpdatePlayer")]
        public async Task<ActionResult> UpdatePlayer(Player player)
        {
           
            await _db.UpdatePlayer(player.Id, player);
            return Ok();
        }
        [HttpPost("RemovePlayer")]
            public async Task<ActionResult> RemovePlayer(string id)
        {
            await _db.RemovePlayer(id);
            return Ok();
        }

    }
}