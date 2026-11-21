using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VT3CyberZone.Server.Data;
using VT3CyberZone.Server.DTOs;
using VT3CyberZone.Server.Security;

namespace VT3CyberZone.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "MISSING_FIELDS", message = "Tên đăng nhập và mật khẩu không được để trống" });

        var username = req.Username.Trim();
        var user = await db.Users
            .Include(u => u.CafeShop)
            .FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { error = "INVALID_CREDENTIALS", message = "Sai tên đăng nhập hoặc mật khẩu" });

        if (user.Status == Models.UserStatus.LOCKED)
            return Unauthorized(new { error = "ACCOUNT_LOCKED", message = "Tài khoản đã bị khóa" });
        if (user.CafeShop.Status != Models.CafeShopStatus.ACTIVE)
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                error = "SHOP_NOT_APPROVED",
                message = user.CafeShop.Status == Models.CafeShopStatus.PENDING
                    ? "Tài khoản quán đang chờ admin nền tảng duyệt."
                    : "Tài khoản quán hiện đang bị tạm ngưng."
            });

        var token = GenerateJwt(user);
        return Ok(new LoginResponse(token, new UserDto(
            user.Id, user.Username, user.FullName, user.Phone,
            user.Role.ToString(), user.Balance, user.Status.ToString(), user.CafeShopId
        )));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var shopId = User.FindFirstValue(CafeShopClaims.ShopId);
        if (!int.TryParse(shopId, out var cafeShopId))
            return Unauthorized();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CafeShopId == cafeShopId);
        if (user == null) return Unauthorized();
        return Ok(new UserDto(user.Id, user.Username, user.FullName, user.Phone,
            user.Role.ToString(), user.Balance, user.Status.ToString(), user.CafeShopId));
    }

    private string GenerateJwt(Models.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(CafeShopClaims.ShopId, user.CafeShopId.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(12),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
