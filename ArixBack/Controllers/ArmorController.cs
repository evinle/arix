using ArixBack.Models;
using ArixBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArixBack.Controllers
{
    [ApiController]
    [Route("Armor")]
    public class ArmorController(ArmorService armorService) : ControllerBase
    {
        [HttpGet("GetAllArmors")]
        public async Task<ActionResult<List<Armor>>> GetAllArmors() =>
            Ok(await armorService.GetArmors());
    }
}
