namespace VT3CyberZone.Server.Models;

public class UsageSession
{
    public int Id { get; set; }
    public int? UserId { get; set; }         // NULL = khách vãng lai
    public int WorkstationId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public decimal TotalCost { get; set; } = 0;
    public SessionStatus Status { get; set; } = SessionStatus.ACTIVE;

    // Navigation
    public User? User { get; set; }
    public Workstation Workstation { get; set; } = null!;
}

public enum SessionStatus { ACTIVE, CLOSED }

