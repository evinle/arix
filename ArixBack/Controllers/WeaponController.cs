using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArixBack.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArixBack.Controllers
{
    [ApiController]
    [Route("Weapons")]
    public class WeaponController : ControllerBase
    {
        private static readonly List<Weapon> weapons = new List<Weapon>
        {
            new Weapon{weaponId=1, weaponName = "la"},
            new Weapon{weaponId=2, weaponName = "lala"}
        };
        
        [HttpGet("GetAll")]
        public ActionResult<List<Weapon>> GetAllPlayers()
        {
            return Ok(weapons);
        }

 

    }
}