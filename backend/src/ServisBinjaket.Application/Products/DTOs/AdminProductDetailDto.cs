namespace ServisBinjaket.Application.Products.DTOs;

public class AdminProductDetailDto
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
    public string Condition { get; set; } = "";
    public int? StockQty { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int? WarrantyMonths { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AdminProductImageDto> Images { get; set; } = [];
}

public class AdminProductImageDto
{
    public int Id { get; set; }
    public string Path { get; set; } = "";
    public string AltSq { get; set; } = "";
    public string? AltEn { get; set; }
    public int SortOrder { get; set; }
}
