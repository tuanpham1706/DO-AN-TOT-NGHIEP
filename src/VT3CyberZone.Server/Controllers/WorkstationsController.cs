using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VT3CyberZone.Server.Data;
using VT3CyberZone.Server.DTOs;
using VT3CyberZone.Server.Models;
using VT3CyberZone.Server.Security;

namespace VT3CyberZone.Server.Controllers;

[ApiController]
[Route("api/workstations")]
[Authorize]
public class WorkstationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status)
    {
        var shopId = User.GetCafeShopId();
        var query = db.Workstations
            .Where(w => w.Group.CafeShopId == shopId)
            .Include(w => w.Group)
            .Include(w => w.Sessions.Where(s => s.Status == SessionStatus.ACTIVE))
            .ThenInclude(s => s.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(w =>
                w.MachineCode.Contains(keyword)
                || (w.IpAddress != null && w.IpAddress.Contains(keyword))
                || w.Group.GroupName.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<WorkstationStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(w => w.Status == parsedStatus);
        }

        var workstations = await query.OrderBy(w => w.MachineCode).ToListAsync();
        var payload = workstations.Select(MapToDto).ToList();
        return Ok(payload);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> GetById(int id)
    {
        var shopId = User.GetCafeShopId();
        var workstation = await db.Workstations
            .Where(w => w.Id == id && w.Group.CafeShopId == shopId)
            .Include(w => w.Group)
            .Include(w => w.Sessions.Where(s => s.Status == SessionStatus.ACTIVE))
            .ThenInclude(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (workstation == null)
            return NotFound(new { message = "Không tìm thấy máy trạm" });

        return Ok(MapToDto(workstation));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> Create([FromBody] CreateWorkstationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MachineCode))
            return BadRequest(new { message = "Mã máy không được để trống" });

        var shopId = User.GetCafeShopId();
        var group = await db.MachineGroups.FirstOrDefaultAsync(
            g => g.Id == request.GroupId && g.CafeShopId == shopId);
        if (group == null)
            return NotFound(new { message = "Nhóm máy không tồn tại" });

        if (await db.Workstations.AnyAsync(w => w.MachineCode == request.MachineCode.Trim()))
            return Conflict(new { message = "Mã máy đã tồn tại" });

        var parsedStatus = Enum.TryParse<WorkstationStatus>(request.Status, true, out var status)
            ? status
            : WorkstationStatus.OFFLINE;

        var workstation = new Workstation
        {
            MachineCode = request.MachineCode.Trim(),
            GroupId = request.GroupId,
            IpAddress = string.IsNullOrWhiteSpace(request.IpAddress) ? null : request.IpAddress.Trim(),
            Status = parsedStatus
        };

        db.Workstations.Add(workstation);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = workstation.Id }, new
        {
            workstation.Id,
            workstation.MachineCode,
            message = "Tạo máy trạm thành công"
        });
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateWorkstationStatusRequest request)
    {
        if (!Enum.TryParse<WorkstationStatus>(request.Status, true, out var nextStatus))
            return BadRequest(new { message = "Trạng thái máy không hợp lệ" });

        var workstation = await FindShopWorkstationAsync(id);
        if (workstation == null)
            return NotFound(new { message = "Không tìm thấy máy trạm" });

        workstation.Status = nextStatus;
        await db.SaveChangesAsync();

        return Ok(new { message = "Cập nhật trạng thái máy thành công", status = workstation.Status.ToString() });
    }

    [HttpPost("{id:int}/lock")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> Lock(int id)
    {
        var workstation = await FindShopWorkstationAsync(id);
        if (workstation == null)
            return NotFound(new { message = "Không tìm thấy máy trạm" });

        workstation.Status = WorkstationStatus.MAINTENANCE;
        await db.SaveChangesAsync();

        return Ok(new { message = "Đã khóa máy trạm", status = workstation.Status.ToString() });
    }

    [HttpPost("{id:int}/unlock")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> Unlock(int id)
    {
        var workstation = await FindShopWorkstationAsync(id);
        if (workstation == null)
            return NotFound(new { message = "Không tìm thấy máy trạm" });

        workstation.Status = WorkstationStatus.IDLE;
        await db.SaveChangesAsync();

        return Ok(new { message = "Đã mở khóa máy trạm", status = workstation.Status.ToString() });
    }

    [HttpPost("{id:int}/restart")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> Restart(int id)
    {
        var workstation = await FindShopWorkstationAsync(id);
        if (workstation == null)
            return NotFound(new { message = "Không tìm thấy máy trạm" });

        workstation.Status = WorkstationStatus.OFFLINE;
        await db.SaveChangesAsync();

        return Ok(new { message = "Đã đặt máy trạm về trạng thái khởi động lại", status = workstation.Status.ToString() });
    }

    private WorkstationDto MapToDto(Workstation workstation)
    {
        var activeSession = workstation.Sessions
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefault();

        ActiveSessionDto? sessionDto = null;
        if (activeSession is not null)
        {
            var elapsedMinutes = Math.Max(0, (int)Math.Ceiling((DateTime.Now - activeSession.StartTime).TotalMinutes));
            sessionDto = new ActiveSessionDto(
                activeSession.Id,
                activeSession.User?.Username ?? "Khách vãng lai",
                activeSession.StartTime,
                elapsedMinutes,
                activeSession.TotalCost,
                activeSession.User?.Balance ?? 0m);
        }

        return new WorkstationDto(
            workstation.Id,
            workstation.MachineCode,
            workstation.Group.GroupName,
            workstation.Group.HourlyRate,
            workstation.IpAddress ?? string.Empty,
            workstation.Status.ToString(),
            sessionDto);
    }

    private Task<Workstation?> FindShopWorkstationAsync(int id)
    {
        var shopId = User.GetCafeShopId();
        return db.Workstations
            .Include(w => w.Group)
            .FirstOrDefaultAsync(w => w.Id == id && w.Group.CafeShopId == shopId);
    }
}
