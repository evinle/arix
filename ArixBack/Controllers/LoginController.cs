using ArixBack.Models;
using ArixBack.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Isopoh.Cryptography.Argon2;
using System.Threading.Tasks;


namespace ArixBack.Controllers
{
    [ApiController]
    [Route("/LoginController")]
    public class LoginController : ControllerBase
    {
        private TokenProvider _db;
        public LoginController(TokenProvider db)
        {
            _db = db;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(LoginModel login)
        {
            if (ValidUserName(login.Username) && ValidPassword(login.Password) && ValidEmail(login.Email))
            {
                await _db.Register(login,LoginType.Normal);
                return Ok();
            }
            return BadRequest("Login Unsuccessful");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel login)
        {
            return await _db.Login(login,LoginType.Normal);
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(LoginModel login)
        {
            //need to implement checks
            return await _db.ChangePassword(login);
        }
        private bool ValidUserName(string username)
        {
            //username policy stuff
            return true;
        }
        private bool ValidPassword(string pwd)
        {
            //password policy stuff
            return true;
        }
        private bool ValidEmail(string email)
        {
            //send email confirmation?
            return true;
        }
    }
}