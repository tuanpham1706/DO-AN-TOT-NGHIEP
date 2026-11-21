namespace VT3CyberZone.Server.Models;

public class Category
{
    public int Id { get; set; }
    public int CafeShopId { get; set; }
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; } = 1;

    // Navigation
    public ICollection<Product> Products { get; set; } = [];
    public CafeShop CafeShop { get; set; } = null!;
}

