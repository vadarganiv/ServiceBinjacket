using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Services.Queries;
using ServisBinjaket.Application.Services.UseCases;

namespace ServisBinjaket.Api.Controllers;

[ApiController]
[Route("api/v1/services")]
public class ServicesController : ControllerBase
{
    private readonly GetServicesUseCase _getServices;
    private readonly GetServiceBySlugUseCase _getBySlug;

    public ServicesController(GetServicesUseCase getServices, GetServiceBySlugUseCase getBySlug)
    {
        _getServices = getServices;
        _getBySlug = getBySlug;
    }

    /// <summary>List published services with optional category filter.</summary>
    [HttpGet]
    public async Task<IActionResult> GetServices(
        [FromQuery] string locale = "sq",
        [FromQuery] int? categoryId = null,
        CancellationToken ct = default)
    {
        var query = new GetServicesQuery { Locale = locale, CategoryId = categoryId };
        var result = await _getServices.ExecuteAsync(query, ct);
        return Ok(result);
    }

    /// <summary>Get service detail by slug.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(
        string slug,
        [FromQuery] string locale = "sq",
        CancellationToken ct = default)
    {
        var result = await _getBySlug.ExecuteAsync(slug, locale, ct);
        if (result is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Service not found" } });

        return Ok(result);
    }
}
