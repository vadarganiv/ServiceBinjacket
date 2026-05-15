using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int? OrderId { get; set; }
    public int? RepairRequestId { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ALL";
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }

    public Order? Order { get; set; }
    public RepairRequest? RepairRequest { get; set; }
}
