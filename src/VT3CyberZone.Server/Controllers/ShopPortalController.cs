using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VT3CyberZone.Server.Data;
using VT3CyberZone.Server.DTOs;
using VT3CyberZone.Server.Models;
using VT3CyberZone.Server.Security;

namespace VT3CyberZone.Server.Controllers;

[ApiController]
[Route("api/shops")]
public class ShopPortalController(
    AppDbContext db,
    IConfiguration config,
    IWebHostEnvironment environment) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCafeShopRequest request)
    {
        var shopName = request.ShopName?.Trim();
        var ownerName = request.OwnerName?.Trim();
        var username = request.Username?.Trim();
        var email = request.Email?.Trim().ToLowerInvariant();
        var passwordBytes = Encoding.UTF8.GetByteCount(request.Password ?? string.Empty);
        if (string.IsNullOrWhiteSpace(shopName)
            || string.IsNullOrWhiteSpace(ownerName)
            || string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(email)
            || !MailAddress.TryCreate(email, out _)
            || passwordBytes is < 8 or > 72)
        {
            return BadRequest(new
            {
                message = "Hãy nhập tên quán, chủ quán, username, email hợp lệ và mật khẩu dài từ 8 đến 72 byte."
            });
        }

        if (await db.CafeShops.AnyAsync(s => s.Email == email))
            return Conflict(new { message = "Email này đã đăng ký quán." });
        if (await db.Users.AnyAsync(u => u.Username == username))
            return Conflict(new { message = "Tên đăng nhập đã được sử dụng." });

        var shop = new CafeShop
        {
            Name = shopName,
            OwnerName = ownerName,
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Status = CafeShopStatus.PENDING
        };
        var user = new User
        {
            CafeShop = shop,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = ownerName,
            Phone = shop.Phone,
            Role = UserRole.ADMIN,
            Status = UserStatus.ACTIVE
        };

        db.CafeShops.Add(shop);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Created("/api/shops/portal/login", new
        {
            shop.Id,
            shop.Name,
            status = shop.Status.ToString(),
            message = "Đăng ký thành công. Quán sẽ sử dụng được sau khi admin nền tảng duyệt."
        });
    }

    [AllowAnonymous]
    [HttpPost("portal/login")]
    public async Task<IActionResult> PortalLogin([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Tên đăng nhập và mật khẩu không được để trống." });

        var owner = await db.Users
            .Include(u => u.CafeShop)
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim() && u.Role == UserRole.ADMIN);
        if (owner is null || !BCrypt.Net.BCrypt.Verify(request.Password, owner.PasswordHash))
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
        if (owner.CafeShop.Status != CafeShopStatus.ACTIVE)
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = owner.CafeShop.Status == CafeShopStatus.PENDING
                    ? "Quán đang chờ admin nền tảng duyệt."
                    : "Quán hiện đang bị tạm ngưng."
            });

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, owner.Id.ToString()),
            new Claim(ClaimTypes.Name, owner.Username),
            new Claim(ClaimTypes.Role, "SHOP_OWNER"),
            new Claim(CafeShopClaims.ShopId, owner.CafeShopId.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);
        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            shop = new
            {
                owner.CafeShop.Id,
                owner.CafeShop.Name,
                status = owner.CafeShop.Status.ToString(),
                owner.CafeShop.LicenseKey,
                owner.CafeShop.ActivatedAt
            }
        });
    }

    [Authorize(Roles = "SHOP_OWNER")]
    [HttpGet("portal/me")]
    public async Task<IActionResult> PortalMe()
    {
        var shopId = User.GetCafeShopId();
        var shop = await db.CafeShops.AsNoTracking().FirstOrDefaultAsync(s => s.Id == shopId);
        if (shop is null)
            return Unauthorized();
        return Ok(new
        {
            shop.Id,
            shop.Name,
            status = shop.Status.ToString(),
            shop.LicenseKey,
            shop.ActivatedAt
        });
    }

    [Authorize(Roles = "SHOP_OWNER")]
    [HttpGet("portal/download")]
    public async Task<IActionResult> DownloadClient()
    {
        var shopId = User.GetCafeShopId();
        var shop = await db.CafeShops.FirstOrDefaultAsync(s => s.Id == shopId);
        if (shop is null || shop.Status != CafeShopStatus.ACTIVE || string.IsNullOrWhiteSpace(shop.LicenseKey))
            return Forbid();

        var configuredPath = config["ClientPackage:Path"];
        if (string.IsNullOrWhiteSpace(configuredPath))
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Bộ cài desktop chưa được cấu hình trên server." });

        var packagePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);
        if (!System.IO.File.Exists(packagePath))
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Không tìm thấy bộ cài desktop trên server." });

        var version = config["ClientPackage:Version"] ?? "unknown";
        db.ShopEvents.Add(new ShopEvent
        {
            CafeShopId = shopId,
            Type = ShopEventType.DOWNLOAD,
            AppVersion = version
        });
        await db.SaveChangesAsync();

        return PhysicalFile(packagePath, "application/octet-stream", Path.GetFileName(packagePath));
    }

    [Authorize(Roles = "ADMIN,STAFF")]
    [HttpPost("activate")]
    public async Task<IActionResult> Activate()
    {
        var shopId = User.GetCafeShopId();
        var shop = await db.CafeShops.FirstOrDefaultAsync(s => s.Id == shopId);
        if (shop is null || shop.Status != CafeShopStatus.ACTIVE || string.IsNullOrWhiteSpace(shop.LicenseKey))
            return Forbid();

        if (shop.ActivatedAt is null)
        {
            shop.ActivatedAt = DateTime.Now;
            db.ShopEvents.Add(new ShopEvent
            {
                CafeShopId = shopId,
                Type = ShopEventType.ACTIVATION,
                AppVersion = config["ClientPackage:Version"]
            });
            await db.SaveChangesAsync();
        }

        return Ok(new { activated = true, shopId, shop.Name, shop.ActivatedAt });
    }
}
