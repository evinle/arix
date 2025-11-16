using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ArixBack.Models;
using ArixBack.Services;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

public class TokenProvider : ControllerBase
{
    private readonly IConfiguration _config;
    private PlayerService _db;

    public TokenProvider(IConfiguration config, PlayerService db)
    {
        _config = config;
        _db = db;
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
        var token = GenerateJwtToken(new Dictionary<string, string>
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
    [HttpGet("yuh")]
    public async Task<IActionResult> yuh(LoginModel login)
    {
        return Ok(await _db.GetPlayerFromEmail(login.Email));
    }
    public async Task<IActionResult> Login(LoginModel login, LoginType tag)
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
                var token = GenerateJwtToken(new Dictionary<string, string> { [JwtRegisteredClaimNames.NameId] = login.Username });
                return Ok(new { token });
            }
            else if (Argon2.Verify(player.Password, login.Password))
            {
                var token = GenerateJwtToken(new Dictionary<string, string> { [JwtRegisteredClaimNames.NameId] = login.Username });
                return Ok(new { token });
            }
        }


        return Unauthorized();
    }

    public async Task<IActionResult> Register(LoginModel login, LoginType loginType)
    {
        Player player = new Player(login.Username, login.Email, Argon2.Hash(login.Password), loginType);
        await _db.CreatePlayer(player);
        return Ok();
    }
    private async Task<IActionResult> RegisterOauth(LoginModel login, LoginType loginType)
    {
        Player player = new Player(login.Username, login.Email, loginType);
        await _db.CreatePlayer(player);
        return Ok();
    }



    public async Task<IActionResult> ChangePassword(LoginModel login)
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

    private string GenerateJwtToken(Dictionary<string, string> claimsToGenerate)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claimsArray = claimsToGenerate.ToArray().Select(x => new Claim(x.Key, x.Value));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        }.Concat(claimsArray);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
