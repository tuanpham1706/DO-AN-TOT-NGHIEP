namespace VT3CyberZone.Server.Models;

/// <summary>Bill of Materials — Định lượng nguyên liệu cho mỗi món</summary>
public class ProductRecipe
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int IngredientId { get; set; }
    public decimal QuantityRequired { get; set; }   // Số lượng cần cho 1 phần

    // Navigation
    public Product Product { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}

