using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Interfaces;

public record OrderItemInput(int ProductId, string NameSnapshot, decimal PriceSnapshot, int Quantity);

public interface IOrderRepository
{
    Task<Order> CreateAsync(Customer customer, Order order, IReadOnlyList<OrderItemInput> items, CancellationToken ct = default);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetAdminListAsync(OrderStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<Order?> GetAdminDetailAsync(int id, CancellationToken ct = default);
}
