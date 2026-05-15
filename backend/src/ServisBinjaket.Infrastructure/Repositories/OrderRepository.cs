using Microsoft.EntityFrameworkCore;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Domain.Enums;
using ServisBinjaket.Infrastructure.Persistence;

namespace ServisBinjaket.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db) => _db = db;

    public async Task<Order> CreateAsync(
        Customer customer,
        Order order,
        IReadOnlyList<OrderItemInput> items,
        CancellationToken ct = default)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        order.CustomerId = customer.Id;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        foreach (var item in items)
        {
            _db.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                NameSnapshot = item.NameSnapshot,
                PriceSnapshot = item.PriceSnapshot,
                Quantity = item.Quantity,
            });
        }
        await _db.SaveChangesAsync(ct);

        return order;
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetAdminListAsync(
        OrderStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .AsQueryable();

        if (status.HasValue)
            q = q.Where(o => o.Status == status.Value);

        var total = await q.CountAsync(ct);

        var clampedPage = Math.Max(1, page);
        var clampedSize = Math.Clamp(pageSize, 1, 100);

        var items = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip((clampedPage - 1) * clampedSize)
            .Take(clampedSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Order?> GetAdminDetailAsync(int id, CancellationToken ct = default)
    {
        return await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<bool> UpdateStatusAsync(int id, OrderStatus status, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([id], ct);
        if (order is null) return false;
        order.Status = status;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateCommentAsync(int id, string? comment, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([id], ct);
        if (order is null) return false;
        order.AdminComment = comment;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<int> CountAsync(CancellationToken ct = default) =>
        _db.Orders.CountAsync(ct);
}
