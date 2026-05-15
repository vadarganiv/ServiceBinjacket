namespace ServisBinjaket.Application.Orders.DTOs;

public class OrderAdminListItemDto
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = "";
    public string CustomerPhone { get; init; } = "";
    public decimal Subtotal { get; init; }
    public string Currency { get; init; } = "ALL";
    public string Status { get; init; } = "";
    public int ItemCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
