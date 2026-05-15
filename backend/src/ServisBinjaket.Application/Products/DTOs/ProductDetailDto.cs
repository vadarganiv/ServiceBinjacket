using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Products.DTOs;

public class ProductDetailDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
    public string ShortDescription { get; init; } = "";
    public string Description { get; init; } = "";
    public decimal Price { get; init; }
    public string Currency { get; init; } = "ALL";
    public ProductCondition Condition { get; init; }
    public int? StockQty { get; init; }
    public int? WarrantyMonths { get; init; }
    public bool InStock { get; init; }
    public ProductCategoryDto? Category { get; init; }
    public IReadOnlyList<ProductImageDto> Images { get; init; } = [];
}
