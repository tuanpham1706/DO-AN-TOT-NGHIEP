using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VT3CyberZone.Server.Data;
using VT3CyberZone.Server.DTOs;
using VT3CyberZone.Server.Security;

namespace VT3CyberZone.Server.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize(Roles = "ADMIN,STAFF")]
public class TransactionsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        if (page < 1 || pageSize is < 1 or > 200)
            return BadRequest(new { message = "Trang phải lớn hơn 0 và pageSize phải từ 1 đến 200." });
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            return BadRequest(new { message = "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc." });

        var shopId = User.GetCafeShopId();
        var query = db.Transactions
            .AsNoTracking()
            .Where(t => t.CafeShopId == shopId)
            .AsQueryable();
        if (from.HasValue)
        {
            var start = from.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(t => t.CreatedAt >= start);
        }
        if (to.HasValue)
        {
            var end = to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(t => t.CreatedAt < end);
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto(
                t.TransCode,
                t.CreatedAt,
                t.User == null ? null : t.User.FullName ?? t.User.Username,
                t.Staff == null ? null : t.Staff.Username,
                t.Type.ToString(),
                t.Amount,
                t.Order == null ? null : t.Order.OrderCode,
                t.Order == null ? null : t.Order.PaymentMethod.ToString()))
            .ToListAsync();

        return Ok(new TransactionPage(total, data));
    }
}
