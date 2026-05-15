using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Domain.Entities;

public class RepairRequest
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int? ServiceId { get; set; }
    public string DeviceType { get; set; } = "";
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string ProblemDescription { get; set; } = "";
    public DeliveryMethod PreferredDeliveryMethod { get; set; }
    public string? CustomerComment { get; set; }
    public RepairStatus Status { get; set; }
    public string? AdminComment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public Service? Service { get; set; }
    public ICollection<RepairRequestFile> Files { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
