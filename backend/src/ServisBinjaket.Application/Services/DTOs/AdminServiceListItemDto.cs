namespace ServisBinjaket.Application.Services.DTOs;

public class AdminServiceListItemDto
{
    public int Id { get; set; }
    public string NameSq { get; set; } = "";
    public string? NameEn { get; set; }
    public string SlugSq { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public bool IsPublished { get; set; }
}
