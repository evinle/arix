using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using ArixBack.Models;
using ArixBack.Services;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

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
        var user = result.Principal!.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        var name = result.Principal!.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        // Generate JWT
        var token = GenerateJwtToken(user);

        // Optionally, clear the temp cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Return JWT to frontend (as JSON)
        return Redirect($"http://localhost:5173/jwtCallback?code={token}");
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);
        return Ok(new { userId, email });
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

    private string GenerateJwtToken(string email)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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
