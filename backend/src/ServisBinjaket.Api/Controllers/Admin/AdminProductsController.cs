using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Products.DTOs;
using ServisBinjaket.Application.Products.UseCases;
using ServisBinjaket.Application.Products.Validators;

namespace ServisBinjaket.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/products")]
[Authorize]
public class AdminProductsController : ControllerBase
{
    private static readonly HashSet<string> AllowedImageMimes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    private readonly IProductRepository _repo;
    private readonly CreateProductUseCase _create;
    private readonly UpdateProductUseCase _update;
    private readonly IValidator<ProductCreateDto> _createValidator;
    private readonly IValidator<ProductUpdateDto> _updateValidator;
    private readonly IConfiguration _config;
    private readonly ILogger<AdminProductsController> _logger;

    public AdminProductsController(
        IProductRepository repo,
        CreateProductUseCase create,
        UpdateProductUseCase update,
        IValidator<ProductCreateDto> createValidator,
        IValidator<ProductUpdateDto> updateValidator,
        IConfiguration config,
        ILogger<AdminProductsController> logger)
    {
        _repo = repo;
        _create = create;
        _update = update;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _config = config;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? q = null,
        [FromQuery] bool? isPublished = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var (products, total) = await _repo.GetAdminListAsync(q, isPublished, page, pageSize, ct);

        var dtos = products.Select(p => new AdminProductListItemDto
        {
            Id = p.Id,
            NameSq = p.NameSq,
            NameEn = p.NameEn,
            SlugSq = p.SlugSq,
            Price = p.Price,
            Currency = p.Currency,
            Condition = p.Condition.ToString(),
            StockQty = p.StockQty,
            IsPublished = p.IsPublished,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.NameSq ?? "",
            ImageCount = p.Images.Count,
            CreatedAt = p.CreatedAt,
        }).ToList();

        return Ok(new { items = dtos, totalCount = total, page, pageSize });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var p = await _repo.GetAdminByIdAsync(id, ct);
        if (p is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Product not found" } });

        return Ok(CreateProductUseCase.MapDetail(p));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Validation failed", details = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }) } });

        var (result, conflict) = await _create.ExecuteAsync(dto, ct);
        if (conflict == "slug")
            return Conflict(new { error = new { code = "CONFLICT", message = "A product with this slug already exists" } });

        _logger.LogInformation("Product created: id={Id}", result!.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto, CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Validation failed", details = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }) } });

        var (result, error) = await _update.ExecuteAsync(id, dto, ct);
        return error switch
        {
            "not_found" => NotFound(new { error = new { code = "NOT_FOUND", message = "Product not found" } }),
            "slug"      => Conflict(new { error = new { code = "CONFLICT", message = "A product with this slug already exists" } }),
            _           => Ok(result)
        };
    }

    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        var ok = await _repo.SetPublishedAsync(id, true, ct);
        return ok ? NoContent() : NotFound(new { error = new { code = "NOT_FOUND", message = "Product not found" } });
    }

    [HttpPost("{id:int}/unpublish")]
    public async Task<IActionResult> Unpublish(int id, CancellationToken ct)
    {
        var ok = await _repo.SetPublishedAsync(id, false, ct);
        return ok ? NoContent() : NotFound(new { error = new { code = "NOT_FOUND", message = "Product not found" } });
    }

    [HttpPost("{id:int}/images")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 30 * 1024 * 1024)]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadImages(int id, IFormFileCollection files, CancellationToken ct)
    {
        var product = await _repo.GetAdminByIdAsync(id, ct);
        if (product is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Product not found" } });

        if (!files.Any())
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "No files provided" } });

        var uploadsRoot = _config["UPLOADS_ROOT"] ?? "/app/uploads";
        if (!int.TryParse(_config["MAX_UPLOAD_MB"], out var maxMb)) maxMb = 10;
        var maxBytes = (long)maxMb * 1024 * 1024;

        var productDir = Path.Combine(uploadsRoot, "products", id.ToString());
        Directory.CreateDirectory(productDir);

        var saved = new List<object>();
        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            if (file.Length > maxBytes)
                return StatusCode(413, new { error = new { code = "FILE_TOO_LARGE", message = $"File exceeds {maxMb} MB limit" } });

            if (!AllowedImageMimes.Contains(file.ContentType))
                return BadRequest(new { error = new { code = "INVALID_FILE_TYPE", message = $"File type '{file.ContentType}' is not allowed" } });

            var ext = Path.GetExtension(file.FileName);
            var guidName = $"{Guid.NewGuid()}{ext}";
            var destPath = Path.GetFullPath(Path.Combine(productDir, guidName));

            if (!destPath.StartsWith(Path.GetFullPath(uploadsRoot), StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = new { code = "INVALID_PATH", message = "Invalid file path" } });

            await using var stream = System.IO.File.Create(destPath);
            await file.CopyToAsync(stream, ct);

            var relativePath = $"/uploads/products/{id}/{guidName}";
            var image = new Domain.Entities.ProductImage
            {
                ProductId = id,
                Path = relativePath,
                AltSq = product.NameSq,
                AltEn = product.NameEn,
            };

            var savedImage = await _repo.AddImageAsync(image, ct);
            saved.Add(new { id = savedImage.Id, path = relativePath, sortOrder = savedImage.SortOrder });
        }

        return Ok(new { uploaded = saved });
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId, CancellationToken ct)
    {
        var ok = await _repo.DeleteImageAsync(imageId, ct);
        return ok ? NoContent() : NotFound(new { error = new { code = "NOT_FOUND", message = "Image not found" } });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var cats = await _repo.GetAllCategoriesAsync(ct);
        return Ok(cats.Select(c => new { c.Id, c.NameSq, c.NameEn }));
    }
}
