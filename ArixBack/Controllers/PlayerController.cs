using Microsoft.AspNetCore.Mvc;
using ArixBack.Models;
using System.Diagnostics;
using System.Data.Common;

namespace ArixBack.Controllers
{
    [ApiController]
    [Route("/players")]
    public class PlayerController : ControllerBase
    {

        private static readonly List<Player> Players = new List<Player>
        {
            new Player{id = 0, username = "L", gold = 0},
            new Player{id = 1, username = "La", gold = 0}
        };

        [HttpGet("GetAllPlayers")]
        public ActionResult<List<Player>> GetAllPlayers()
        {
            return Ok(Players);
        }

        [HttpGet("GetPlayer")]
        public ActionResult<List<Player>> GetPlayers(int id)
        {
            Player? player = Players.FirstOrDefault(x => x.id == id);

            if (player == null)
            {
                return NotFound();
            }
            return Ok(player);
        }


    }
}