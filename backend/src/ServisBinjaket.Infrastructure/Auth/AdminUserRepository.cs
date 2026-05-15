using Microsoft.EntityFrameworkCore;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Infrastructure.Persistence;

namespace ServisBinjaket.Infrastructure.Auth;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly AppDbContext _db;

    public AdminUserRepository(AppDbContext db) => _db = db;

    public Task<AdminUser?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _db.AdminUsers.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<AdminUser?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.AdminUsers.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task UpdateAsync(AdminUser user, CancellationToken ct = default)
    {
        _db.AdminUsers.Update(user);
        await _db.SaveChangesAsync(ct);
    }
}
