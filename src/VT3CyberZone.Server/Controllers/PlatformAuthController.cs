using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VT3CyberZone.Server.Data;
using VT3CyberZone.Server.DTOs;

namespace VT3CyberZone.Server.Controllers;

[ApiController]
[Route("api/platform/auth")]
public class PlatformAuthController(AppDbContext db, IConfiguration config) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] PlatformLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Tên đăng nhập và mật khẩu không được để trống." });

        var username = request.Username.Trim();
        var admin = await db.PlatformAdmins.FirstOrDefaultAsync(a => a.Username == username && a.IsActive);
        if (admin is null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, "PLATFORM_ADMIN")
        };
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return Ok(new PlatformLoginResponse(new JwtSecurityTokenHandler().WriteToken(token), admin.Username));
    }

    [Authorize(Roles = "PLATFORM_ADMIN")]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new { username = User.Identity?.Name, role = "PLATFORM_ADMIN" });
}
