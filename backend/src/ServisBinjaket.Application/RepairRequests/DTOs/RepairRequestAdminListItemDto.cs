namespace ServisBinjaket.Application.RepairRequests.DTOs;

public class RepairRequestAdminListItemDto
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = "";
    public string CustomerPhone { get; init; } = "";
    public string DeviceType { get; init; } = "";
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public string Status { get; init; } = "";
    public int FileCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
