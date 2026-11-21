using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VT3CyberZone.Server.Data;
using VT3CyberZone.Server.DTOs;
using VT3CyberZone.Server.Models;
using VT3CyberZone.Server.Security;

namespace VT3CyberZone.Server.Controllers;

[ApiController]
[Route("api/members")]
[Authorize]
public class MembersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            return BadRequest(new { message = "Trang phải lớn hơn 0 và pageSize phải từ 1 đến 100." });

        var shopId = User.GetCafeShopId();
        var query = db.Users
            .Where(u => u.CafeShopId == shopId)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.Username.Contains(search)
                || (u.FullName != null && u.FullName.Contains(search))
                || (u.Phone != null && u.Phone.Contains(search)));

        var total = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new UserDto(
                u.Id,
                u.Username,
                u.FullName,
                u.Phone,
                u.Role.ToString(),
                u.Balance,
                u.Status.ToString(),
                u.CafeShopId))
            .ToListAsync();

        return Ok(new { total, data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var shopId = User.GetCafeShopId();
        if (User.IsInRole(nameof(UserRole.MEMBER))
            && !string.Equals(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                id.ToString(),
                StringComparison.Ordinal))
            return Forbid();

        var u = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.CafeShopId == shopId);
        if (u == null) return NotFound(new { message = "Không tìm thấy tài khoản" });
        return Ok(new UserDto(u.Id, u.Username, u.FullName, u.Phone, u.Role.ToString(), u.Balance, u.Status.ToString(), u.CafeShopId));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest req)
    {
        var shopId = User.GetCafeShopId();
        if (await db.Users.AnyAsync(u => u.Username == req.Username && u.CafeShopId == shopId))
            return Conflict(new { error = "USERNAME_EXISTS", message = "Tên đăng nhập đã tồn tại" });

        var user = new User
        {
            CafeShopId = shopId,
            Username = req.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FullName = req.FullName,
            Phone = req.Phone,
            Role = UserRole.MEMBER
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Created($"/api/members/{user.Id}", new { user.Id, message = "Tạo tài khoản thành công" });
    }

    [HttpPost("{id:int}/topup")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> TopUp(int id, [FromBody] TopUpRequest req)
    {
        if (req.Amount <= 0 || req.Amount > 10_000_000)
            return BadRequest(new { error = "INVALID_AMOUNT", message = "Số tiền nạp phải từ 1 đến 10.000.000 đồng" });

        var shopId = User.GetCafeShopId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.CafeShopId == shopId);
        if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản" });

        user.Balance += req.Amount;

        var trans = new Transaction
        {
            TransCode = $"VT3C-{Guid.NewGuid():N}"[..25],
            CafeShopId = shopId,
            UserId = user.Id,
            StaffId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value),
            Type = TransactionType.TOP_UP,
            Amount = req.Amount
        };
        db.Transactions.Add(trans);
        await db.SaveChangesAsync();

        return Ok(new TopUpResponse(user.Balance, trans.TransCode, "Nạp tiền thành công"));
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateMemberStatusRequest request)
    {
        if (!Enum.TryParse<UserStatus>(request.Status, true, out var newStatus))
            return BadRequest(new { error = "INVALID_STATUS", message = "Trạng thái không hợp lệ. Giá trị cho phép: ACTIVE, LOCKED." });

        var shopId = User.GetCafeShopId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.CafeShopId == shopId);
        if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản" });
        user.Status = newStatus;
        await db.SaveChangesAsync();
        return Ok(new { message = "Cập nhật trạng thái thành công", status = user.Status.ToString() });
    }

    [HttpPatch("{id:int}/password")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        // Chỉ ADMIN/STAFF hoặc chính user đó mới được đổi mật khẩu
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isStaffOrAdmin = User.IsInRole("ADMIN") || User.IsInRole("STAFF");
        if (!isStaffOrAdmin && currentUserId != id.ToString())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest(new { error = "WEAK_PASSWORD", message = "Mật khẩu mới phải có ít nhất 6 ký tự." });

        var shopId = User.GetCafeShopId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.CafeShopId == shopId);
        if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản" });

        // STAFF/ADMIN phải cung cấp mật khẩu cũ trừ khi là ADMIN reset
        if (!User.IsInRole("ADMIN") && !string.IsNullOrWhiteSpace(request.OldPassword))
        {
            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                return BadRequest(new { error = "WRONG_PASSWORD", message = "Mật khẩu cũ không đúng" });
        }
        else if (!User.IsInRole("ADMIN"))
        {
            if (string.IsNullOrWhiteSpace(request.OldPassword) ||
                !BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                return BadRequest(new { error = "WRONG_PASSWORD", message = "Mật khẩu cũ không đúng" });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await db.SaveChangesAsync();
        return Ok(new { message = "Đổi mật khẩu thành công" });
    }
}
