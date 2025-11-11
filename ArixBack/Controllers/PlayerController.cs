using Microsoft.AspNetCore.Mvc;
using ArixBack.Models;
using System.Diagnostics;
using System.Data.Common;
using ArixBack.Services;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
namespace ArixBack.Controllers
{
    //[Authorize]
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

        [HttpGet("GetPlayerFromId")]
        public async Task<ActionResult<Player>> GetPlayerFromId(string id)
        {
            var player = await _db.GetPlayerFromId(id);
            if (player == null) return NotFound($"No player with ID: {id}");
            return Ok(player);
        }


        [HttpGet("GetPlayerFromUsername")]
        public async Task<ActionResult<Player>> GetPlayerFromUsername(string username)
        {
            var player = await _db.GetPlayerFromUsername(username);
            if (player == null) return NotFound($"No player with ID: {username}");
            return Ok(player);
        }
        [HttpPost("CreatePlayer")]
        public async Task<ActionResult> CreatePlayer(Player player)
        {
            if (player == null) return BadRequest("No player provided");

            player.Id = null;
            await _db.CreatePlayer(player);
            return Ok();
        }
        [HttpPost("UpdatePlayer")]
        public async Task<ActionResult> UpdatePlayer(Player player)
        {
            if (player == null || player.Id == null) return BadRequest("Invalid player");
            await _db.UpdatePlayer(player.Id, player);
            return Ok();
        }
        [HttpPost("RemovePlayer")]
            public async Task<ActionResult> RemovePlayer(string id)
        {
            var deletedRow = await _db.RemovePlayer(id);
            if (deletedRow.DeletedCount == 0) return BadRequest($"Could not delete player with ID: {id}");
            return Ok(deletedRow.ToJson());
        }

    }
}