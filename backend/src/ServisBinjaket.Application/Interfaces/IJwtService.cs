namespace ServisBinjaket.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(int adminId, string email);
}
