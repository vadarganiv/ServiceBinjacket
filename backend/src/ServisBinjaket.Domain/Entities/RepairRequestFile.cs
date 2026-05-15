namespace ServisBinjaket.Domain.Entities;

public class RepairRequestFile
{
    public int Id { get; set; }
    public int RepairRequestId { get; set; }
    public string Path { get; set; } = "";
    public string OriginalName { get; set; } = "";
    public string MimeType { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }

    public RepairRequest RepairRequest { get; set; } = null!;
}
