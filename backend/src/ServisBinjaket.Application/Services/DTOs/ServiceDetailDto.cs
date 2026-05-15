namespace ServisBinjaket.Application.Services.DTOs;

public class ServiceDetailDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
    public string ShortDescription { get; init; } = "";
    public string Description { get; init; } = "";
    public string? PriceNote { get; init; }
    public ServiceCategoryDto? Category { get; init; }
}
