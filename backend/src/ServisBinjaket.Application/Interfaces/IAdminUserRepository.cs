using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Interfaces;

public interface IAdminUserRepository
{
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<AdminUser?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(AdminUser user, CancellationToken ct = default);
}
