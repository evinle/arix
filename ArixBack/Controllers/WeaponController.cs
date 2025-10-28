using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArixBack.Models;
using ArixBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArixBack.Controllers
{
    [ApiController]
    [Route("Weapons")]
    public class WeaponController : ControllerBase
    {
        private DatabaseService _db;
        public WeaponController(DatabaseService db)
        {
            _db = db;
        }
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

        [HttpPost("InsertWeapon")]
        public async Task<ActionResult> InsertWeapon(int id, String weaponName)
        {
            Weapon weapon = new Weapon(id, weaponName);
            await _db.CreateWeapon(weapon);
            return Ok();     
        }
 

    }
}