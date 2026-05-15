using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Products.DTOs;

public class AdminProductListItemDto
{
    public int Id { get; set; }
    public string NameSq { get; set; } = "";
    public string? NameEn { get; set; }
    public string SlugSq { get; set; } = "";
    public decimal Price { get; set; }
    public string Currency { get; set; } = "ALL";
    public string Condition { get; set; } = "";
    public int? StockQty { get; set; }
    public bool IsPublished { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int ImageCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
