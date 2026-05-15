using Microsoft.EntityFrameworkCore;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Services.Queries;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Infrastructure.Persistence;

namespace ServisBinjaket.Infrastructure.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _db;

    public ServiceRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Service>> GetServicesAsync(GetServicesQuery query, CancellationToken ct = default)
    {
        var q = _db.Services
            .Include(s => s.Category)
            .Where(s => s.IsPublished)
            .AsQueryable();

        if (query.CategoryId.HasValue)
            q = q.Where(s => s.CategoryId == query.CategoryId.Value);

        return await q
            .OrderBy(s => s.Category.SortOrder)
            .ThenBy(s => s.Id)
            .ToListAsync(ct);
    }

    public async Task<Service?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.Services
            .Include(s => s.Category)
            .Where(s => s.IsPublished && (s.SlugSq == slug || s.SlugEn == slug))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<ServiceCategory>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await _db.ServiceCategories
            .Where(c => c.IsPublished)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);
    }
}
