using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArixBack.Models;
using ArixBack.Services;
using Microsoft.VisualBasic;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;

public class TokenProvider : ControllerBase
{

    private readonly IConfiguration _config;
    private PlayerService _db;
    public TokenProvider(IConfiguration config, PlayerService db)
    {
        _config = config;
        _db = db;
    }

    [HttpGet("signin-google")]
    public IActionResult SignInWithGoogle()
    {
        var redirectUrl = Url.Action("callback-google", "Account", null, Request.Scheme);
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    // http://localhost:5115/callback-google
    [HttpGet("callback-google")]
    public async Task<IActionResult> CallbackFromGoogle()
    {
       // Authenticate using the temporary cookie
    var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    if (!result.Succeeded)
        return BadRequest("Login failed");

    // Optional: access claims
    var claims = result.Principal!.Claims;

    // Sign in user for your own app session
    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal!, result.Properties);

    return RedirectToAction("Index", "Home"); 
    }

    public async Task<IActionResult> Login(LoginModel login)
    {
        var player = await _db.GetPlayerFromUsername(login.Username);

        if (player != null && Argon2.Verify(player.Password, login.Password))
        {
            var token = GenerateJwtToken(login.Username);
            return Ok(new { token });
        }

        return Unauthorized();
    }

    public async Task<IActionResult> Register(LoginModel login)
    {
        Player player = new Player(login.Username, login.Email, Argon2.Hash(login.Password));
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

    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

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