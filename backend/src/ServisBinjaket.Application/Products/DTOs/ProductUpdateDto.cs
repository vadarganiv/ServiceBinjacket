using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Products.DTOs;

public class ProductUpdateDto
{
    public string NameSq { get; set; } = "";
    public string? NameEn { get; set; }
    public string? SlugSq { get; set; }
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
}
