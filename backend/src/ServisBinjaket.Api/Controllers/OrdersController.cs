using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Orders.DTOs;
using ServisBinjaket.Application.Orders.UseCases;

namespace ServisBinjaket.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly CreateOrderUseCase _create;
    private readonly IValidator<OrderCreateDto> _validator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        CreateOrderUseCase create,
        IValidator<OrderCreateDto> validator,
        ILogger<OrdersController> logger)
    {
        _create = create;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>Submit a new order (cash-only).</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrderCreateDto dto, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "Validation failed",
                    details = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                }
            });
        }

        try
        {
            var result = await _create.ExecuteAsync(dto, ct);
            _logger.LogInformation("Order created: id={Id}, items={Count}", result.Id, dto.Items.Count);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = new { code = "VALIDATION_ERROR", message = ex.Message }
            });
        }
    }
}
