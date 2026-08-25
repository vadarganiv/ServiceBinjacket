using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Api.Uploads;
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
    private const long MaxTransportBytes = 32L * 1024 * 1024;

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
            "slug" => Conflict(new { error = new { code = "CONFLICT", message = "A product with this slug already exists" } }),
            _ => Ok(result)
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
    [RequestFormLimits(MultipartBodyLengthLimit = MaxTransportBytes)]
    [RequestSizeLimit(MaxTransportBytes)]
    public async Task<IActionResult> UploadImages(
        int id,
        [FromForm] IFormFileCollection files,
        CancellationToken ct)
    {
        var product = await _repo.GetAdminByIdAsync(id, ct);
        if (product is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Product not found" } });

        var candidates = files.Where(file => file.Length > 0).ToList();
        if (candidates.Count == 0)
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "No files provided" } });

        const int maxFiles = 8;
        if (candidates.Count > maxFiles)
            return BadRequest(new { error = new { code = "TOO_MANY_FILES", message = $"A maximum of {maxFiles} images can be uploaded at once" } });

        if (!int.TryParse(_config["MAX_UPLOAD_MB"], out var maxMb)) maxMb = 10;
        maxMb = Math.Clamp(maxMb, 1, 25);
        var maxBytes = (long)maxMb * 1024 * 1024;

        var validatedFiles = new List<(IFormFile File, ValidatedUpload Metadata)>();
        foreach (var file in candidates)
        {
            if (file.Length > maxBytes)
                return StatusCode(413, new { error = new { code = "FILE_TOO_LARGE", message = $"File exceeds {maxMb} MB limit" } });

            var validation = await UploadFileValidator.ValidateAsync(file, UploadFileScope.ImagesOnly, ct);
            if (!validation.IsValid)
            {
                return BadRequest(new
                {
                    error = new
                    {
                        code = validation.ErrorCode,
                        message = validation.ErrorMessage
                    }
                });
            }

            validatedFiles.Add((file, validation.Upload!));
        }

        var uploadsRoot = Path.GetFullPath(_config["UPLOADS_ROOT"] ?? "/app/uploads");
        var productDir = Path.Combine(uploadsRoot, "products", id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Directory.CreateDirectory(productDir);

        var saved = new List<object>();
        foreach (var (file, metadata) in validatedFiles)
        {
            var guidName = $"{Guid.NewGuid():N}{metadata.StorageExtension}";
            var destPath = Path.Combine(productDir, guidName);

            var relativePath = $"/uploads/products/{id}/{guidName}";
            var image = new Domain.Entities.ProductImage
            {
                ProductId = id,
                Path = relativePath,
                AltSq = product.NameSq,
                AltEn = product.NameEn,
            };

            Domain.Entities.ProductImage savedImage;
            try
            {
                await using (var stream = new FileStream(
                                 destPath,
                                 FileMode.CreateNew,
                                 FileAccess.Write,
                                 FileShare.None,
                                 bufferSize: 81920,
                                 FileOptions.Asynchronous))
                {
                    await file.CopyToAsync(stream, ct);
                }

                savedImage = await _repo.AddImageAsync(image, ct);
            }
            catch
            {
                TryDeleteFile(destPath);
                throw;
            }

            saved.Add(new { id = savedImage.Id, path = relativePath, sortOrder = savedImage.SortOrder });
        }

        return Ok(new { uploaded = saved });
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId, CancellationToken ct)
    {
        var storedPath = await _repo.DeleteImageAsync(id, imageId, ct);
        if (storedPath is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Image not found" } });

        var uploadsRoot = Path.GetFullPath(_config["UPLOADS_ROOT"] ?? "/app/uploads");
        var productDirectory = Path.GetFullPath(Path.Combine(
            uploadsRoot,
            "products",
            id.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        var fileName = Path.GetFileName(storedPath);
        var physicalPath = Path.GetFullPath(Path.Combine(productDirectory, fileName));

        if (physicalPath.StartsWith(
                productDirectory + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            TryDeleteFile(physicalPath);
        }

        return NoContent();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var cats = await _repo.GetAllCategoriesAsync(ct);
        return Ok(cats.Select(c => new { c.Id, c.NameSq, c.NameEn }));
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            System.IO.File.Delete(path);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to remove incomplete upload at {Path}", path);
        }
    }
}
