namespace ServisBinjaket.Application.Services.DTOs;

public class ServiceUpdateDto
{
    public string NameSq { get; set; } = "";
    public string? NameEn { get; set; }
    public string? SlugSq { get; set; }
    public string? SlugEn { get; set; }
    public string ShortDescriptionSq { get; set; } = "";
    public string? ShortDescriptionEn { get; set; }
    public string DescriptionSq { get; set; } = "";
    public string? DescriptionEn { get; set; }
    public string PriceNoteSq { get; set; } = "";
    public string? PriceNoteEn { get; set; }
    public int CategoryId { get; set; }
}
