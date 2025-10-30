using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArixBack.Models;
using ArixBack.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace ArixBack.Controllers
{
    [ApiController]
    [Route("Weapons")]
    public class WeaponController(WeaponService weaponService) : ControllerBase
    {
        private WeaponService _weaponService => weaponService;

        [HttpGet("GetAllWeapons")]
        public async Task<ActionResult<List<Weapon>>> GetAllWeapons()
        {
            return Ok(await _weaponService.GetWeapons());
        }

        [HttpGet("GetWeapon")]
        public async Task<ActionResult<Weapon>> GetWeapon(int id)
        {
            var wep = await _weaponService.GetWeapon(id);

            if (wep == null) return NotFound($"No weapon with ID: {id}");

            return Ok(wep);
        }

        [HttpPost("CreateWeapon")]
        public async Task<ActionResult> InsertWeapon([FromBody] Weapon newWeapon)
        {
            if (newWeapon == null) return BadRequest("No Weapon provided");
 
            await _weaponService.CreateWeapon(newWeapon);
            return Ok();     
        }
        [HttpPost("UpdateWeapon")]
        public async Task<ActionResult> UpdateWeapon([FromBody] Weapon wep)
        {

            if (wep == null) return BadRequest("No Weapon provided");

            var updatedRow = await _weaponService.UpdateWeapon(wep.Id,wep);

            if (updatedRow == false) return BadRequest($"Could not update {wep}");

            return NoContent();     
        }
        [HttpPost("RemoveWeapon")]
        public async Task<ActionResult> RemoveWeapon(int id)
        {
            var deletedRow = await _weaponService.RemoveWeapon(id);

            if (deletedRow.DeletedCount == 0) return BadRequest($"Could not delete weapon with ID: {id}");

            return Ok(deletedRow.ToJson());
        }
 

    }
}