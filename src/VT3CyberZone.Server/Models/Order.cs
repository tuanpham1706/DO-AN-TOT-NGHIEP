namespace VT3CyberZone.Server.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;      // VT3F-0001
    public int? UserId { get; set; }
    public int WorkstationId { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.PENDING;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public User? User { get; set; }
    public Workstation Workstation { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
}

public enum PaymentMethod { CASH, ACCOUNT_BALANCE }
public enum OrderStatus { PENDING, PREPARING, COMPLETED, CANCELLED }

