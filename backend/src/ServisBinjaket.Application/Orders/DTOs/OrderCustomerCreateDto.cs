namespace ServisBinjaket.Application.Orders.DTOs;

public class OrderCustomerCreateDto
{
    public string FullName { get; init; } = "";
    public string Phone { get; init; } = "";
    public string? WhatsAppPhone { get; init; }
    public string City { get; init; } = "";
    public string? Address { get; init; }
}
