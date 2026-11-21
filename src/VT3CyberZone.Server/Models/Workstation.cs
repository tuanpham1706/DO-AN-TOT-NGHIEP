namespace VT3CyberZone.Server.Models;

public class Workstation
{
    public int Id { get; set; }
    public string MachineCode { get; set; } = null!;
    public int GroupId { get; set; }
    public string? IpAddress { get; set; }
    public WorkstationStatus Status { get; set; } = WorkstationStatus.OFFLINE;

    // Navigation
    public MachineGroup Group { get; set; } = null!;
    public ICollection<UsageSession> Sessions { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}

public enum WorkstationStatus { OFFLINE, IDLE, IN_USE, MAINTENANCE }

