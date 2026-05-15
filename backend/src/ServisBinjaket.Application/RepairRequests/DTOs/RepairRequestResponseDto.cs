using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.RepairRequests.DTOs;

public class RepairRequestResponseDto
{
    public int Id { get; init; }
    public RepairStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
