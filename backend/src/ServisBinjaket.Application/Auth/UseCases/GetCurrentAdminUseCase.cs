using ServisBinjaket.Application.Auth.DTOs;
using ServisBinjaket.Application.Interfaces;

namespace ServisBinjaket.Application.Auth.UseCases;

public class GetCurrentAdminUseCase
{
    private readonly IAdminUserRepository _repo;

    public GetCurrentAdminUseCase(IAdminUserRepository repo) => _repo = repo;

    public async Task<AdminMeDto?> ExecuteAsync(int adminId, CancellationToken ct = default)
    {
        var admin = await _repo.GetByIdAsync(adminId, ct);
        if (admin is null || !admin.IsActive)
            return null;

        return new AdminMeDto
        {
            Id = admin.Id,
            Email = admin.Email,
            DisplayName = admin.DisplayName,
            LastLoginAt = admin.LastLoginAt
        };
    }
}
