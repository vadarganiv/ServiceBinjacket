namespace ServisBinjaket.Application.RepairRequests.DTOs;

public class CustomerCreateDto
{
    public string FullName { get; init; } = "";
    public string Phone { get; init; } = "";
    public string? WhatsAppPhone { get; init; }
    public string City { get; init; } = "";
}
