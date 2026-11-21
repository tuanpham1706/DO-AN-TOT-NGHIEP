using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VT3CyberZone.Server.Data;
using VT3CyberZone.Server.DTOs;
using VT3CyberZone.Server.Models;
using VT3CyberZone.Server.Security;

namespace VT3CyberZone.Server.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public class UsageSessionsController(AppDbContext db) : ControllerBase
{
    [HttpGet("active")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> GetActiveSessions()
    {
        var shopId = User.GetCafeShopId();
        var sessions = await db.UsageSessions
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Workstation)
            .ThenInclude(w => w.Group)
            .Where(s => s.Status == SessionStatus.ACTIVE && s.Workstation.Group.CafeShopId == shopId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        var payload = sessions.Select(s => new
        {
            s.Id,
            s.WorkstationId,
            MachineCode = s.Workstation.MachineCode,
            MemberName = s.User?.Username ?? "Khách vãng lai",
            Balance = s.User?.Balance ?? 0m,
            StartTime = s.StartTime,
            MinutesElapsed = Math.Max(0, (int)Math.Ceiling((DateTime.Now - s.StartTime).TotalMinutes)),
            CostSoFar = s.TotalCost,
            Rate = s.Workstation.Group.HourlyRate
        });

        return Ok(payload);
    }

    [HttpPost("open")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> OpenSession([FromBody] OpenSessionRequest request)
    {
        var shopId = User.GetCafeShopId();
        var workstation = await db.Workstations
            .Include(w => w.Group)
            .FirstOrDefaultAsync(w => w.Id == request.WorkstationId && w.Group.CafeShopId == shopId);

        if (workstation == null)
            return NotFound(new { message = "Không tìm thấy máy trạm" });

        var hasActiveSession = await db.UsageSessions.AnyAsync(
            s => s.WorkstationId == request.WorkstationId && s.Status == SessionStatus.ACTIVE);
        if (hasActiveSession)
            return Conflict(new { message = "Máy trạm này đang có phiên hoạt động" });

        User? member = null;
        if (request.MemberId.HasValue)
        {
            member = await db.Users.FirstOrDefaultAsync(
                u => u.Id == request.MemberId.Value && u.CafeShopId == shopId);
            if (member == null)
                return NotFound(new { message = "Không tìm thấy tài khoản khách hàng" });

            if (member.Status == UserStatus.LOCKED)
                return BadRequest(new { message = "Tài khoản khách hàng đang bị khóa" });
        }

        var session = new UsageSession
        {
            UserId = request.MemberId,
            WorkstationId = workstation.Id,
            StartTime = DateTime.Now,
            Status = SessionStatus.ACTIVE,
            TotalCost = 0m
        };

        db.UsageSessions.Add(session);
        workstation.Status = WorkstationStatus.IN_USE;
        await db.SaveChangesAsync();

        return Ok(new OpenSessionResponse(
            session.Id,
            workstation.MachineCode,
            member?.Username ?? "Khách vãng lai",
            member?.Balance ?? 0m,
            0,
            "Mở phiên thành công"));
    }

    [HttpPost("{id:int}/close")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> CloseSession(int id)
    {
        var shopId = User.GetCafeShopId();

        // Dùng ExecuteUpdate để atomic-lock phiên trước, tránh race condition
        var closed = await db.UsageSessions
            .Where(s => s.Id == id
                     && s.Status == SessionStatus.ACTIVE
                     && s.Workstation.Group.CafeShopId == shopId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, SessionStatus.CLOSED));

        if (closed == 0)
        {
            // Kiểm tra xem phiên tồn tại không (để phân biệt NotFound vs đã đóng)
            var exists = await db.UsageSessions
                .AnyAsync(s => s.Id == id && s.Workstation.Group.CafeShopId == shopId);
            return exists
                ? BadRequest(new { message = "Phiên chơi này đã được đóng trước đó." })
                : NotFound(new { message = "Không tìm thấy phiên chơi." });
        }

        // Tải lại phiên sau khi đã khóa thành công
        var session = await db.UsageSessions
            .Include(s => s.User)
            .Include(s => s.Workstation).ThenInclude(w => w.Group)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
            return NotFound(new { message = "Không tìm thấy phiên chơi." });

        var now = DateTime.Now;
        var totalMinutes = Math.Max(1, (int)Math.Ceiling((now - session.StartTime).TotalMinutes));
        var hourlyRate = session.Workstation.Group.HourlyRate;
        var totalCost = Math.Round((totalMinutes / 60m) * hourlyRate, 2, MidpointRounding.AwayFromZero);
        var remainingBalance = session.User?.Balance ?? 0m;

        string transCode = "N/A";

        if (session.UserId.HasValue)
        {
            var user = session.User ?? await db.Users.FirstOrDefaultAsync(
                u => u.Id == session.UserId.Value && u.CafeShopId == shopId);
            if (user == null)
                return NotFound(new { message = "Tài khoản khách hàng không còn tồn tại." });

            // Cho phép trừ tiền, nhưng ghi log nếu số dư âm (phòng net tự quyết định chính sách)
            user.Balance -= totalCost;
            remainingBalance = user.Balance;

            transCode = $"VT3C-{Guid.NewGuid():N}"[..25];
            db.Transactions.Add(new Transaction
            {
                TransCode = transCode,
                CafeShopId = shopId,
                UserId = user.Id,
                StaffId = GetCurrentStaffId(),
                Type = TransactionType.GAME_FEE,
                Amount = totalCost,
                CreatedAt = now
            });
        }

        session.EndTime = now;
        session.TotalCost = totalCost;
        // Status đã được set CLOSED ở bước atomic trên
        session.Workstation.Status = WorkstationStatus.IDLE;
        await db.SaveChangesAsync();

        return Ok(new CloseSessionResponse(
            session.Id,
            totalMinutes,
            totalCost,
            remainingBalance,
            transCode));
    }

    private int? GetCurrentStaffId()
    {
        var staffValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(staffValue, out var staffId) ? staffId : null;
    }
}
