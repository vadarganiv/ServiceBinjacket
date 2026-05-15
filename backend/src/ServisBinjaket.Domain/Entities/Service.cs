namespace ServisBinjaket.Domain.Entities;

public class Service
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
    public string PriceNoteSq { get; set; } = "";
    public string? PriceNoteEn { get; set; }
    public int CategoryId { get; set; }
    public bool IsPublished { get; set; }

    public ServiceCategory Category { get; set; } = null!;
    public ICollection<RepairRequest> RepairRequests { get; set; } = [];
}
