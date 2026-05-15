using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string NameSq { get; set; } = "";
    public string? NameEn { get; set; }
    public string SlugSq { get; set; } = "";
    public string? SlugEn { get; set; }
    public string ShortDescriptionSq { get; set; } = "";
    public string? ShortDescriptionEn { get; set; }
    public string DescriptionSq { get; set; } = "";
    public string? DescriptionEn { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "ALL";
    public ProductCondition Condition { get; set; }
    public int? StockQty { get; set; }
    public int CategoryId { get; set; }
    public int? WarrantyMonths { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ProductCategory Category { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
