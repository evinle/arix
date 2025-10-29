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
        private WeaponService _db;
        public WeaponController(WeaponService db)
        {
            _db = db;
        }

        [HttpGet("GetAllWeapons")]
        public async Task<ActionResult<List<Weapon>>> GetAllWeapons()
        {
            return Ok(await _db.GetWeapons());
        }
        [HttpGet("GetWeapon")]
        public async Task<ActionResult<Weapon>> GetWeapon(int id)
        {
            return Ok(await _db.GetWeapon(id));
        }

        [HttpPost("CreateWeapon")]
        public async Task<ActionResult> InsertWeapon(int id, string weaponName)
        {
            Weapon weapon = new Weapon(id, weaponName);
            await _db.CreateWeapon(weapon);
            return Ok();     
        }
        [HttpPost("UpdateWeapon")]
        public async Task<ActionResult> UpdateWeapon(int id, string weaponName)
        {
            Weapon weapon = new Weapon(id, weaponName);
            await _db.UpdateWeapon(id,weapon);
            return Ok();     
        }
        [HttpPost("RemoveWeapon")]
        public async Task<ActionResult> RemoveWeapon(int id)
        {
            await _db.RemoveWeapon(id);
            return Ok();     
        }
 

    }
}