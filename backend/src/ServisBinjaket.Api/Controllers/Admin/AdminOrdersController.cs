using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Orders.DTOs;
using ServisBinjaket.Domain.Enums;
using System.Security.Claims;

namespace ServisBinjaket.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/orders")]
[Authorize]
public class AdminOrdersController : ControllerBase
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<AdminOrdersController> _logger;

    public AdminOrdersController(IOrderRepository repository, ILogger<AdminOrdersController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>List all orders for an authenticated administrator.</summary>
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

    /// <summary>Get order detail for an authenticated administrator.</summary>
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

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto, CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(dto.Status, ignoreCase: true, out var status))
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Invalid status value" } });

        var ok = await _repository.UpdateStatusAsync(id, status, ct);
        if (!ok)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Order not found" } });

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Order {OrderId} status changed to {Status} by admin {AdminId}", id, status, adminId);
        return NoContent();
    }

    [HttpPut("{id:int}/comment")]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto dto, CancellationToken ct)
    {
        var ok = await _repository.UpdateCommentAsync(id, dto.Comment, ct);
        if (!ok)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Order not found" } });

        return NoContent();
    }
}
