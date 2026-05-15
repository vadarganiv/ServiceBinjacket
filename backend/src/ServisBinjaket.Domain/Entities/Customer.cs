namespace ServisBinjaket.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? WhatsAppPhone { get; set; }
    public string? Email { get; set; }
    public string City { get; set; } = "";
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<RepairRequest> RepairRequests { get; set; } = [];
}
