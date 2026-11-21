using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VT3CyberZone.Server.Data;
using VT3CyberZone.Server.DTOs;
using VT3CyberZone.Server.Models;
using VT3CyberZone.Server.Security;

namespace VT3CyberZone.Server.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "ADMIN,STAFF")]
public class ReportsController(AppDbContext db) : ControllerBase
{
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var fromDate = from ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-6));
        var toDate = to ?? DateOnly.FromDateTime(DateTime.Today);
        if (fromDate > toDate)
            return BadRequest(new { message = "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc." });

        var start = fromDate.ToDateTime(TimeOnly.MinValue);
        var end = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var shopId = User.GetCafeShopId();
        var transactions = await db.Transactions
            .AsNoTracking()
            .Where(t => t.CafeShopId == shopId && t.CreatedAt >= start && t.CreatedAt < end
                && (t.Type == TransactionType.GAME_FEE || t.Type == TransactionType.FOOD_ORDER))
            .Select(t => new { t.CreatedAt, t.Type, t.Amount })
            .ToListAsync();

        var gameFeeRevenue = transactions
            .Where(t => t.Type == TransactionType.GAME_FEE)
            .Sum(t => t.Amount);
        var foodRevenue = transactions
            .Where(t => t.Type == TransactionType.FOOD_ORDER)
            .Sum(t => t.Amount);
        var byDay = transactions
            .GroupBy(t => DateOnly.FromDateTime(t.CreatedAt))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var game = g.Where(t => t.Type == TransactionType.GAME_FEE).Sum(t => t.Amount);
                var food = g.Where(t => t.Type == TransactionType.FOOD_ORDER).Sum(t => t.Amount);
                return new DailyRevenueDto(g.Key.ToString("yyyy-MM-dd"), game, food, game + food);
            })
            .ToList();

        return Ok(new RevenueResponse(
            gameFeeRevenue + foodRevenue,
            gameFeeRevenue,
            foodRevenue,
            byDay));
    }
}
