namespace VT3CyberZone.Server.Models;

public class MachineGroup
{
    public int Id { get; set; }
    public int CafeShopId { get; set; }
    public string GroupName { get; set; } = null!;
    public decimal HourlyRate { get; set; }
    public string? Specs { get; set; }

    // Navigation
    public ICollection<Workstation> Workstations { get; set; } = [];
    public CafeShop CafeShop { get; set; } = null!;
}

