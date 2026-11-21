namespace VT3CyberZone.Server.Models;

public class PlatformAdmin
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class SubscriptionPayment
{
    public int Id { get; set; }
    public int CafeShopId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.Now;
    public string? Note { get; set; }
    public int RecordedByAdminId { get; set; }

    public CafeShop CafeShop { get; set; } = null!;
}

public class ShopEvent
{
    public int Id { get; set; }
    public int CafeShopId { get; set; }
    public ShopEventType Type { get; set; }
    public string? AppVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public CafeShop CafeShop { get; set; } = null!;
}

public enum ShopEventType { DOWNLOAD, ACTIVATION }
