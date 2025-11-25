using ArixBack.Models;
using ArixBack.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Isopoh.Cryptography.Argon2;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;


namespace ArixBack.Controllers
{
    [ApiController]
    [Route("/LoginController")]
    public class LoginController : ControllerBase
    {
        private PlayerService _db;
        private TokenProvider _tokenProvider;
        public LoginController(PlayerService db, TokenProvider tokenProvider)
        {
            _db = db;
            _tokenProvider = tokenProvider;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(LoginModel login)
        {
            if (ValidUserName(login.Username) && ValidPassword(login.Password) && ValidEmail(login.Email))
            {
                await RegisterIntoDb(login, LoginType.Normal);
                return Ok();
            }
            return BadRequest("Login Unsuccessful");
        }

        private async Task<IActionResult> RegisterIntoDb(LoginModel login, LoginType loginType)
        {
            Player player = new Player(login.Username, login.Email, Argon2.Hash(login.Password), loginType);
            await _db.CreatePlayer(player);
            return Ok();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel login)
        {
            return await AuthenticateLogin(login, LoginType.Normal);
        }

        private async Task<IActionResult> AuthenticateLogin(LoginModel login, LoginType tag)
        {

            Player? player = null;
            if (tag == LoginType.Google)
            {
                player = await _db.GetPlayerFromEmail(login.Email);
            }
            else
            {
                player = await _db.GetPlayerFromUsername(login.Username);
            }

            if (player != null)
            {

                if (tag == LoginType.Google)
                {
                    var token = _tokenProvider.GenerateJwtToken(new Dictionary<string, string> { [JwtRegisteredClaimNames.NameId] = login.Username });
                    return Ok(new { token });
                }
                else if (Argon2.Verify(player.Password, login.Password))
                {
                    var token = _tokenProvider.GenerateJwtToken(new Dictionary<string, string> { [JwtRegisteredClaimNames.NameId] = login.Username });
                    return Ok(new { token });
                }
            }


            return Unauthorized();
        }


        [HttpGet("oauth")]
        public IActionResult Oauth()
        {
            var redirectUrl = Url.Action("ReturnToUser");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("callback-google")]
        public async Task<IActionResult> ReturnToUser()
        {
            // Authenticate using the temporary cookie
            var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            if (!result.Succeeded)
                return BadRequest("Login failed");

            // Get claims from the external login
            var email = result.Principal!.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var name = result.Principal!.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var picture = result.Principal!.Claims.FirstOrDefault(x => x.Type == "urn:google:picture")?.Value; ;

            //add to database, check if exist in database   
            var emailCheck = await _db.GetPlayerFromEmail(email);
            if (emailCheck == null)
            {
                LoginModel loginModel = new LoginModel { Username = name, Email = email };
                await RegisterOauth(loginModel, LoginType.Google);
            }

            // Generate JWT
            var token = _tokenProvider.GenerateJwtToken(new Dictionary<string, string>
            {
                [JwtRegisteredClaimNames.Email] = email,
                [JwtRegisteredClaimNames.Name] = name,
                [JwtRegisteredClaimNames.Picture] = picture
            });

            // Optionally, clear the temp cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Return JWT to frontend (as JSON)
            return Redirect($"http://localhost:5173/jwtCallback?code={token}");
        }

        private async Task<IActionResult> RegisterOauth(LoginModel login, LoginType loginType)
        {
            Player player = new Player(login.Username, login.Email, loginType);
            await _db.CreatePlayer(player);
            return Ok();
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(LoginModel login)
        {

            //need to implement checks
            //do basic security check
            return await ChangePassword(login);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userName = User.FindFirstValue(JwtRegisteredClaimNames.NameId);
            var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);
            var picture = User.FindFirstValue(JwtRegisteredClaimNames.Picture);
            var name = User.FindFirstValue(JwtRegisteredClaimNames.Name);
            var id = (await _db.GetPlayerFromEmail(email))?.Id;
            return Ok(new { userName, email, picture, id, name });
        }

        private async Task<IActionResult> ChangePassword(LoginModel login)
        {
            Player? player = await _db.GetPlayerFromUsername(login.Username);
            if (player != null && player.Id != null)
            {
                player.Password = Argon2.Hash(login.Password);
                await _db.UpdatePlayer(player.Id, player);

                return Ok(new { player });
            }
            return Forbid();
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