using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Services.UseCases;

namespace ServisBinjaket.Api.Controllers;

[ApiController]
[Route("api/v1/service-categories")]
public class ServiceCategoriesController : ControllerBase
{
    private readonly GetServiceCategoriesUseCase _getCategories;

    public ServiceCategoriesController(GetServiceCategoriesUseCase getCategories)
    {
        _getCategories = getCategories;
    }

    /// <summary>List published service categories.</summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories(
        [FromQuery] string locale = "sq",
        CancellationToken ct = default)
    {
        var result = await _getCategories.ExecuteAsync(locale, ct);
        return Ok(result);
    }
}
