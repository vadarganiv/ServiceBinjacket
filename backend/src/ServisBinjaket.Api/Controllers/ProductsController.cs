using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Products.Queries;
using ServisBinjaket.Application.Products.UseCases;
using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly GetProductsUseCase _getProducts;
    private readonly GetProductBySlugUseCase _getBySlug;

    public ProductsController(GetProductsUseCase getProducts, GetProductBySlugUseCase getBySlug)
    {
        _getProducts = getProducts;
        _getBySlug = getBySlug;
    }

    /// <summary>List published products with optional filtering and pagination.</summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string locale = "sq",
        [FromQuery] int? categoryId = null,
        [FromQuery] string? q = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] ProductCondition? condition = null,
        [FromQuery] bool? inStock = null,
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetProductsQuery
        {
            Locale = locale,
            CategoryId = categoryId,
            Q = q,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Condition = condition,
            InStock = inStock,
            Sort = sort,
            Page = page,
            PageSize = pageSize
        };

        var result = await _getProducts.ExecuteAsync(query, ct);
        return Ok(result);
    }

    /// <summary>Get product detail by slug.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(
        string slug,
        [FromQuery] string locale = "sq",
        CancellationToken ct = default)
    {
        var result = await _getBySlug.ExecuteAsync(slug, locale, ct);
        if (result is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Product not found" } });

        return Ok(result);
    }
}
