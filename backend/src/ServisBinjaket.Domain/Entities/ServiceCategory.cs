namespace ServisBinjaket.Domain.Entities;

public class ServiceCategory
{
    public int Id { get; set; }
    public string NameSq { get; set; } = "";
    public string? NameEn { get; set; }
    public string SlugSq { get; set; } = "";
    public string? SlugEn { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }

    public ICollection<Service> Services { get; set; } = [];
}
