using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Orders.DTOs;

public class OrderCreateDto
{
    public OrderCustomerCreateDto Customer { get; init; } = new();
    public DeliveryMethod DeliveryMethod { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public IReadOnlyList<OrderItemCreateDto> Items { get; init; } = [];
    public string? CustomerComment { get; init; }
    public bool Consent { get; init; }
}
