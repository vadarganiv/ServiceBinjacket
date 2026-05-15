using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Products.DTOs;

public class ProductListItemDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
    public string ShortDescription { get; init; } = "";
    public decimal Price { get; init; }
    public string Currency { get; init; } = "ALL";
    public ProductCondition Condition { get; init; }
    public ProductImageDto? Image { get; init; }
    public bool InStock { get; init; }
}
