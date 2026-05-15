namespace ServisBinjaket.Domain.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Path { get; set; } = "";
    public string AltSq { get; set; } = "";
    public string? AltEn { get; set; }
    public int SortOrder { get; set; }

    public Product Product { get; set; } = null!;
}
