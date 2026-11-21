namespace VT3CyberZone.Server.Models;

public class User
{
    public int Id { get; set; }
    public int CafeShopId { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.MEMBER;
    public decimal Balance { get; set; } = 0;
    public UserStatus Status { get; set; } = UserStatus.ACTIVE;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<UsageSession> Sessions { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public CafeShop CafeShop { get; set; } = null!;
}

public enum UserRole { ADMIN, STAFF, MEMBER }
public enum UserStatus { ACTIVE, LOCKED }

