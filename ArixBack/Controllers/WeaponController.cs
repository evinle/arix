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
        private WeaponService _WeaponService => weaponService;

        [HttpGet("GetAllWeapons")]
        public async Task<ActionResult<List<Weapon>>> GetAllWeapons()
        {
            return Ok(await _WeaponService.GetWeapons());
        }

        [HttpGet("GetWeapon")]
        public async Task<ActionResult<Weapon>> GetWeapon(string id)
        {
            var wep = await _WeaponService.GetWeapon(id);

            if (wep == null) return NotFound($"No weapon with ID: {id}");

            return Ok(wep);
        }

        [HttpPost("CreateWeapon")]
        public async Task<ActionResult> InsertWeapon(Weapon newWeapon)
        {
            if (newWeapon == null) return BadRequest("No Weapon provided");
            newWeapon.Id = null;

            await _WeaponService.CreateWeapon(newWeapon);
            return Ok();
        }
        [HttpPost("UpdateWeapon")]
        public async Task<ActionResult> UpdateWeapon([FromBody] Weapon wep)
        {

            if (wep == null) return BadRequest("No Weapon provided");
            if (wep.Id == null) return BadRequest("Id not provided");

            var updatedRow = await _WeaponService.UpdateWeapon(wep.Id, wep);

            if (updatedRow == false) return BadRequest($"Could not update {wep}");

            return NoContent();
        }
        [HttpPost("RemoveWeapon")]
        public async Task<ActionResult> RemoveWeapon(string id)
        {
            var deletedRow = await _WeaponService.RemoveWeapon(id);

            if (deletedRow.DeletedCount == 0) return BadRequest($"Could not delete weapon with ID: {id}");

            return Ok(deletedRow.ToJson());
        }


    }
}