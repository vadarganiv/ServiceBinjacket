using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Products.UseCases;

namespace ServisBinjaket.Api.Controllers;

[ApiController]
[Route("api/v1/product-categories")]
public class ProductCategoriesController : ControllerBase
{
    private readonly GetProductCategoriesUseCase _getCategories;

    public ProductCategoriesController(GetProductCategoriesUseCase getCategories)
    {
        _getCategories = getCategories;
    }

    /// <summary>List published product categories.</summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories(
        [FromQuery] string locale = "sq",
        CancellationToken ct = default)
    {
        var result = await _getCategories.ExecuteAsync(locale, ct);
        return Ok(result);
    }
}
