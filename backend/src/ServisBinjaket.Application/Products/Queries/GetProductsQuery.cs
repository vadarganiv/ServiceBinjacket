using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Products.Queries;

public record GetProductsQuery
{
    public string Locale { get; init; } = "sq";
    public int? CategoryId { get; init; }
    public string? Q { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public ProductCondition? Condition { get; init; }
    public bool? InStock { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
