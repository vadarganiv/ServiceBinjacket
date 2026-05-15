using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Orders.DTOs;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Orders.UseCases;

public class CreateOrderUseCase
{
    private readonly IOrderRepository _orders;
    private readonly IProductRepository _products;

    public CreateOrderUseCase(IOrderRepository orders, IProductRepository products)
    {
        _orders = orders;
        _products = products;
    }

    public async Task<OrderResponseDto> ExecuteAsync(OrderCreateDto dto, CancellationToken ct = default)
    {
        var requestedIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _products.GetByIdsAsync(requestedIds, ct);

        var productMap = products.ToDictionary(p => p.Id);
        var missingIds = requestedIds.Except(productMap.Keys).ToList();
        if (missingIds.Count > 0)
            throw new InvalidOperationException($"Products not found or unavailable: {string.Join(", ", missingIds)}");

        var itemInputs = dto.Items
            .Select(i => new OrderItemInput(
                ProductId: i.ProductId,
                NameSnapshot: productMap[i.ProductId].NameSq,
                PriceSnapshot: productMap[i.ProductId].Price,
                Quantity: i.Quantity))
            .ToList();

        var subtotal = itemInputs.Sum(i => i.PriceSnapshot * i.Quantity);

        var customer = new Customer
        {
            FullName = dto.Customer.FullName,
            Phone = dto.Customer.Phone,
            WhatsAppPhone = dto.Customer.WhatsAppPhone,
            City = dto.Customer.City,
            Address = dto.Customer.Address,
        };

        var order = new Order
        {
            DeliveryMethod = dto.DeliveryMethod,
            PaymentMethod = dto.PaymentMethod,
            Status = OrderStatus.New,
            Subtotal = subtotal,
            Currency = "ALL",
            CustomerComment = dto.CustomerComment,
        };

        var saved = await _orders.CreateAsync(customer, order, itemInputs, ct);

        return new OrderResponseDto { Id = saved.Id, Status = saved.Status, CreatedAt = saved.CreatedAt };
    }
}
