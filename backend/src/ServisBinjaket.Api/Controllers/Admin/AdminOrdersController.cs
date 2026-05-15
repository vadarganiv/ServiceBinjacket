using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Orders.DTOs;
using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/orders")]
public class AdminOrdersController : ControllerBase
{
    private readonly IOrderRepository _repository;

    public AdminOrdersController(IOrderRepository repository) => _repository = repository;

    /// <summary>List all orders (admin). TODO: requires auth (TASK-012).</summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var (orders, total) = await _repository.GetAdminListAsync(status, page, pageSize, ct);

        var dtos = orders.Select(o => new OrderAdminListItemDto
        {
            Id = o.Id,
            CustomerName = o.Customer.FullName,
            CustomerPhone = o.Customer.Phone,
            Subtotal = o.Subtotal,
            Currency = o.Currency,
            Status = o.Status.ToString(),
            ItemCount = o.Items.Count,
            CreatedAt = o.CreatedAt,
        }).ToList();

        return Ok(new { items = dtos, totalCount = total, page, pageSize });
    }

    /// <summary>Get order detail (admin). TODO: requires auth (TASK-012).</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var order = await _repository.GetAdminDetailAsync(id, ct);
        if (order is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Order not found" } });

        return Ok(new
        {
            id = order.Id,
            status = order.Status.ToString(),
            deliveryMethod = order.DeliveryMethod.ToString(),
            paymentMethod = order.PaymentMethod.ToString(),
            subtotal = order.Subtotal,
            currency = order.Currency,
            customerComment = order.CustomerComment,
            adminComment = order.AdminComment,
            createdAt = order.CreatedAt,
            updatedAt = order.UpdatedAt,
            customer = new
            {
                fullName = order.Customer.FullName,
                phone = order.Customer.Phone,
                whatsAppPhone = order.Customer.WhatsAppPhone,
                city = order.Customer.City,
                address = order.Customer.Address,
            },
            items = order.Items.Select(i => new
            {
                id = i.Id,
                productId = i.ProductId,
                nameSnapshot = i.NameSnapshot,
                priceSnapshot = i.PriceSnapshot,
                quantity = i.Quantity,
                lineTotal = i.PriceSnapshot * i.Quantity,
            }),
        });
    }
}
