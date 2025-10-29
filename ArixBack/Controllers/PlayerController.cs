using Microsoft.AspNetCore.Mvc;
using ArixBack.Models;
using System.Diagnostics;
using System.Data.Common;
using ArixBack.Services;
using MongoDB.Driver;
using System.Threading.Tasks;
namespace ArixBack.Controllers
{
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
        public async Task<ActionResult<Player>> GetPlayers(int id)
        {
            return Ok(await _db.GetPlayer(id));
        }
        [HttpGet("CreatePlayer")]
        public async Task<ActionResult> CreatePlayer(int id,string username, int gold)
        {
            Player player = new Player(id, username, gold);
            await _db.CreatePlayer(player);
            return Ok();
        }
        [HttpGet("UpdatePlayer")]
        public async Task<ActionResult> UpdatePlayer(int id,string username, int gold)
        {
            Player player = new Player(id, username, gold);
            await _db.UpdatePlayer(id, player);
            return Ok();
        }
        [HttpGet("RemovePlayer")]
        public async Task<ActionResult> RemovePlayer(int id)
        {
            await _db.RemovePlayer(id);
            return Ok();
        }

    }
}