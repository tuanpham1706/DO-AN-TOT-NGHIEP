using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VT3CyberZone.Server.Data;
using VT3CyberZone.Server.DTOs;
using VT3CyberZone.Server.Models;

namespace VT3CyberZone.Server.Controllers;

[ApiController]
[Route("api/platform")]
[Authorize(Roles = "PLATFORM_ADMIN")]
public class PlatformShopsController(AppDbContext db) : ControllerBase
{
    [HttpGet("shops")]
    public async Task<IActionResult> GetShops([FromQuery] string? status)
    {
        var query = db.CafeShops.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<CafeShopStatus>(status, true, out var parsedStatus))
                return BadRequest(new { message = "Trạng thái quán không hợp lệ." });
            query = query.Where(s => s.Status == parsedStatus);
        }

        var shops = await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new CafeShopDto(
                s.Id,
                s.Name,
                s.OwnerName,
                s.Email,
                s.Phone,
                s.Status.ToString(),
                s.LicenseKey,
                s.CreatedAt,
                s.ApprovedAt,
                s.Events.Count(e => e.Type == ShopEventType.DOWNLOAD),
                s.Events.Count(e => e.Type == ShopEventType.ACTIVATION)))
            .ToListAsync();
        return Ok(shops);
    }

    [HttpPost("shops/{id:int}/approve")]
    public async Task<IActionResult> ApproveShop(int id)
    {
        var shop = await db.CafeShops.FirstOrDefaultAsync(s => s.Id == id);
        if (shop is null)
            return NotFound(new { message = "Không tìm thấy quán." });
        if (shop.Status != CafeShopStatus.PENDING)
            return Conflict(new { message = "Chỉ có thể duyệt quán đang chờ xử lý." });

        shop.Status = CafeShopStatus.ACTIVE;
        shop.ApprovedAt = DateTime.Now;
        shop.LicenseKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        await db.SaveChangesAsync();
        return Ok(new { shop.Id, shop.Status, shop.LicenseKey, shop.ApprovedAt });
    }

    [HttpPost("shops/{id:int}/suspend")]
    public async Task<IActionResult> SuspendShop(int id)
    {
        var shop = await db.CafeShops.FirstOrDefaultAsync(s => s.Id == id);
        if (shop is null)
            return NotFound(new { message = "Không tìm thấy quán." });
        if (shop.Status == CafeShopStatus.PENDING)
            return BadRequest(new { message = "Hãy từ chối hoặc duyệt đăng ký đang chờ; không thể tạm ngưng quán chưa được duyệt." });

        shop.Status = CafeShopStatus.SUSPENDED;
        await db.SaveChangesAsync();
        return Ok(new { shop.Id, status = shop.Status.ToString() });
    }

    [HttpPost("shops/{id:int}/payments")]
    public async Task<IActionResult> RecordPayment(int id, [FromBody] RecordSubscriptionPaymentRequest request)
    {
        if (request.Amount <= 0 || request.Amount > 1_000_000_000m)
            return BadRequest(new { message = "Số tiền phải lớn hơn 0 và không vượt quá 1.000.000.000 đồng." });
        if (request.PeriodStart > request.PeriodEnd)
            return BadRequest(new { message = "Ngày bắt đầu kỳ phải trước hoặc bằng ngày kết thúc." });

        var shop = await db.CafeShops.FirstOrDefaultAsync(s => s.Id == id);
        if (shop is null)
            return NotFound(new { message = "Không tìm thấy quán." });

        var adminIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(adminIdValue, out var adminId))
            return Unauthorized();

        var payment = new SubscriptionPayment
        {
            CafeShopId = shop.Id,
            Amount = request.Amount,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            PaidAt = request.PaidAt ?? DateTime.Now,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            RecordedByAdminId = adminId
        };
        db.SubscriptionPayments.Add(payment);
        await db.SaveChangesAsync();

        return Ok(ToDto(payment, shop.Name));
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            return BadRequest(new { message = "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc." });

        var query = db.SubscriptionPayments
            .AsNoTracking()
            .Include(p => p.CafeShop)
            .AsQueryable();
        if (from.HasValue)
        {
            var start = from.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(p => p.PaidAt >= start);
        }
        if (to.HasValue)
        {
            var end = to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(p => p.PaidAt < end);
        }

        var payments = await query
            .OrderByDescending(p => p.PaidAt)
            .Select(p => new SubscriptionPaymentDto(
                p.Id,
                p.CafeShopId,
                p.CafeShop.Name,
                p.Amount,
                p.PeriodStart,
                p.PeriodEnd,
                p.PaidAt,
                p.Note))
            .ToListAsync();
        return Ok(new SubscriptionRevenueResponse(payments.Sum(p => p.Amount), payments));
    }

    private static SubscriptionPaymentDto ToDto(SubscriptionPayment payment, string shopName)
        => new(
            payment.Id,
            payment.CafeShopId,
            shopName,
            payment.Amount,
            payment.PeriodStart,
            payment.PeriodEnd,
            payment.PaidAt,
            payment.Note);
}
