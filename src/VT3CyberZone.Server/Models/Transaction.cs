namespace VT3CyberZone.Server.Models;

public class Transaction
{
    public int Id { get; set; }
    public int CafeShopId { get; set; }
    public string TransCode { get; set; } = null!;  // VT3C-0001
    public int? UserId { get; set; }
    public int? StaffId { get; set; }
    public int? OrderId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public User? User { get; set; }
    public User? Staff { get; set; }
    public Order? Order { get; set; }
    public CafeShop CafeShop { get; set; } = null!;
}

public enum TransactionType { TOP_UP, GAME_FEE, FOOD_ORDER, REFUND }

