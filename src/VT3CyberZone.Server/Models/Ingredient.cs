namespace VT3CyberZone.Server.Models;

public class Ingredient
{
    public int Id { get; set; }
    public int CafeShopId { get; set; }
    public string Name { get; set; } = null!;
    public string Unit { get; set; } = null!;       // gói, quả, gram, lon, chai
    public decimal StockQuantity { get; set; } = 0;
    public decimal MinThreshold { get; set; } = 5;  // Cảnh báo khi dưới ngưỡng này

    // Navigation
    public ICollection<ProductRecipe> Recipes { get; set; } = [];
    public CafeShop CafeShop { get; set; } = null!;

    // Computed
    public bool IsLowStock => StockQuantity <= MinThreshold;
}

