using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public string Currency { get; set; } = "ALL";
    public string? CustomerComment { get; set; }
    public string? AdminComment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
