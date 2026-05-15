namespace ServisBinjaket.Application.Auth.DTOs;

public class AdminMeDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public DateTime? LastLoginAt { get; set; }
}
