using ServisBinjaket.Application.Interfaces;

namespace ServisBinjaket.Infrastructure.Services;

public class HealthService : IHealthService
{
    public string GetStatus() => "healthy";
}
