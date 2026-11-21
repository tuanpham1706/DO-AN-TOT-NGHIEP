namespace VT3CyberZone.Server.Models;

public class CafeShop
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string OwnerName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public CafeShopStatus Status { get; set; } = CafeShopStatus.PENDING;
    public string? LicenseKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }

    public ICollection<User> Users { get; set; } = [];
    public ICollection<SubscriptionPayment> SubscriptionPayments { get; set; } = [];
    public ICollection<ShopEvent> Events { get; set; } = [];
}

public enum CafeShopStatus { PENDING, ACTIVE, SUSPENDED }
