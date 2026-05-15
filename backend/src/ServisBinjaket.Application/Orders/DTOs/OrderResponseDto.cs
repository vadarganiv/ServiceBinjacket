using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Orders.DTOs;

public class OrderResponseDto
{
    public int Id { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
