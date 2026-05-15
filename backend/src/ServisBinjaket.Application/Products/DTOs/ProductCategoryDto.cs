namespace ServisBinjaket.Application.Products.DTOs;

public class ProductCategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public int? ParentId { get; init; }
    public int SortOrder { get; init; }
}
