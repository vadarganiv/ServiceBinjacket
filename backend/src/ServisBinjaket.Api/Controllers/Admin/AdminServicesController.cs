using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Services.DTOs;
using ServisBinjaket.Application.Services.UseCases;

namespace ServisBinjaket.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/services")]
[Authorize]
public class AdminServicesController : ControllerBase
{
    private readonly IServiceRepository _repo;
    private readonly CreateServiceUseCase _create;
    private readonly UpdateServiceUseCase _update;
    private readonly IValidator<ServiceCreateDto> _createValidator;
    private readonly IValidator<ServiceUpdateDto> _updateValidator;
    private readonly ILogger<AdminServicesController> _logger;

    public AdminServicesController(
        IServiceRepository repo,
        CreateServiceUseCase create,
        UpdateServiceUseCase update,
        IValidator<ServiceCreateDto> createValidator,
        IValidator<ServiceUpdateDto> updateValidator,
        ILogger<AdminServicesController> logger)
    {
        _repo = repo;
        _create = create;
        _update = update;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? q = null,
        [FromQuery] bool? isPublished = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var (services, total) = await _repo.GetAdminListAsync(q, isPublished, page, pageSize, ct);

        var dtos = services.Select(s => new AdminServiceListItemDto
        {
            Id = s.Id,
            NameSq = s.NameSq,
            NameEn = s.NameEn,
            SlugSq = s.SlugSq,
            CategoryId = s.CategoryId,
            CategoryName = s.Category?.NameSq ?? "",
            IsPublished = s.IsPublished,
        }).ToList();

        return Ok(new { items = dtos, totalCount = total, page, pageSize });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var s = await _repo.GetAdminByIdAsync(id, ct);
        if (s is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Service not found" } });

        return Ok(CreateServiceUseCase.MapDetail(s));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ServiceCreateDto dto, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Validation failed", details = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }) } });

        var (result, conflict) = await _create.ExecuteAsync(dto, ct);
        if (conflict == "slug")
            return Conflict(new { error = new { code = "CONFLICT", message = "A service with this slug already exists" } });

        _logger.LogInformation("Service created: id={Id}", result!.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ServiceUpdateDto dto, CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Validation failed", details = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }) } });

        var (result, error) = await _update.ExecuteAsync(id, dto, ct);
        return error switch
        {
            "not_found" => NotFound(new { error = new { code = "NOT_FOUND", message = "Service not found" } }),
            "slug" => Conflict(new { error = new { code = "CONFLICT", message = "A service with this slug already exists" } }),
            _ => Ok(result)
        };
    }

    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        var ok = await _repo.SetPublishedAsync(id, true, ct);
        return ok ? NoContent() : NotFound(new { error = new { code = "NOT_FOUND", message = "Service not found" } });
    }

    [HttpPost("{id:int}/unpublish")]
    public async Task<IActionResult> Unpublish(int id, CancellationToken ct)
    {
        var ok = await _repo.SetPublishedAsync(id, false, ct);
        return ok ? NoContent() : NotFound(new { error = new { code = "NOT_FOUND", message = "Service not found" } });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var cats = await _repo.GetAllCategoriesAsync(ct);
        return Ok(cats.Select(c => new { c.Id, c.NameSq, c.NameEn }));
    }
}
