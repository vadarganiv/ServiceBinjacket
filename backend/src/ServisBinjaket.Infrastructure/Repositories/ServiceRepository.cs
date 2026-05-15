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

    // ── Admin ──────────────────────────────────────────────────────────────────

    public async Task<(IReadOnlyList<Service> Items, int TotalCount)> GetAdminListAsync(
        string? q, bool? isPublished, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Services
            .Include(s => s.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.ToLower();
            query = query.Where(s =>
                s.NameSq.ToLower().Contains(search) ||
                (s.NameEn != null && s.NameEn.ToLower().Contains(search)));
        }

        if (isPublished.HasValue)
            query = query.Where(s => s.IsPublished == isPublished.Value);

        var total = await query.CountAsync(ct);
        var clampedPage = Math.Max(1, page);
        var clampedSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderBy(s => s.Category.SortOrder)
            .ThenBy(s => s.Id)
            .Skip((clampedPage - 1) * clampedSize)
            .Take(clampedSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Service?> GetAdminByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Services
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Service> CreateAsync(Service service, CancellationToken ct = default)
    {
        _db.Services.Add(service);
        await _db.SaveChangesAsync(ct);
        return service;
    }

    public async Task UpdateAsync(Service service, CancellationToken ct = default)
    {
        _db.Services.Update(service);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> SetPublishedAsync(int id, bool isPublished, CancellationToken ct = default)
    {
        var service = await _db.Services.FindAsync([id], ct);
        if (service is null) return false;
        service.IsPublished = isPublished;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SlugExistsAsync(string slugSq, string? slugEn, int? excludeId, CancellationToken ct = default)
    {
        var q = _db.Services.AsQueryable();
        if (excludeId.HasValue)
            q = q.Where(s => s.Id != excludeId.Value);

        return await q.AnyAsync(s =>
            s.SlugSq == slugSq ||
            (slugEn != null && (s.SlugSq == slugEn || s.SlugEn == slugEn)) ||
            (s.SlugEn != null && s.SlugEn == slugSq), ct);
    }

    public async Task<IReadOnlyList<ServiceCategory>> GetAllCategoriesAsync(CancellationToken ct = default)
    {
        return await _db.ServiceCategories
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(CancellationToken ct = default) =>
        _db.Services.CountAsync(ct);
}
