using Microsoft.EntityFrameworkCore;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Domain.Enums;
using ServisBinjaket.Infrastructure.Persistence;

namespace ServisBinjaket.Infrastructure.Repositories;

public class RepairRequestRepository : IRepairRequestRepository
{
    private readonly AppDbContext _db;

    public RepairRequestRepository(AppDbContext db) => _db = db;

    public async Task<RepairRequest> CreateAsync(Customer customer, RepairRequest request, CancellationToken ct = default)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        request.CustomerId = customer.Id;
        _db.RepairRequests.Add(request);
        await _db.SaveChangesAsync(ct);

        return request;
    }

    public async Task<RepairRequest?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.RepairRequests
            .Include(r => r.Customer)
            .Include(r => r.Service)
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task AddFileAsync(RepairRequestFile file, CancellationToken ct = default)
    {
        _db.RepairRequestFiles.Add(file);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<RepairRequest> Items, int TotalCount)> GetAdminListAsync(
        RepairStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.RepairRequests
            .Include(r => r.Customer)
            .Include(r => r.Files)
            .AsQueryable();

        if (status.HasValue)
            q = q.Where(r => r.Status == status.Value);

        var total = await q.CountAsync(ct);

        var clampedPage = Math.Max(1, page);
        var clampedSize = Math.Clamp(pageSize, 1, 100);

        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((clampedPage - 1) * clampedSize)
            .Take(clampedSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<RepairRequest?> GetAdminDetailAsync(int id, CancellationToken ct = default)
    {
        return await _db.RepairRequests
            .Include(r => r.Customer)
            .Include(r => r.Service)
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }
}
