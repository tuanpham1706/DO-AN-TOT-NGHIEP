namespace VT3CyberZone.Server.Models;

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation
    public Category Category { get; set; } = null!;
    public ICollection<ProductRecipe> Recipes { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

