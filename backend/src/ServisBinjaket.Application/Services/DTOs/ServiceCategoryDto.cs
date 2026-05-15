namespace ServisBinjaket.Application.Services.DTOs;

public class ServiceCategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public int SortOrder { get; init; }
}
