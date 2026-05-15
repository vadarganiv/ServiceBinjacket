using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.RepairRequests.DTOs;

public class RepairRequestCreateDto
{
    public CustomerCreateDto Customer { get; init; } = new();
    public int? ServiceId { get; init; }
    public string DeviceType { get; init; } = "";
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public string ProblemDescription { get; init; } = "";
    public DeliveryMethod PreferredDeliveryMethod { get; init; }
    public string? CustomerComment { get; init; }
    public bool Consent { get; init; }
}
