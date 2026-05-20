using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.RepairRequests.DTOs;
using ServisBinjaket.Application.RepairRequests.UseCases;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Api.Controllers;

[ApiController]
[Route("api/v1/repair-requests")]
public class RepairRequestsController : ControllerBase
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "video/mp4", "application/pdf"
    };

    private readonly CreateRepairRequestUseCase _create;
    private readonly IRepairRequestRepository _repository;
    private readonly IValidator<RepairRequestCreateDto> _validator;
    private readonly IConfiguration _config;
    private readonly ILogger<RepairRequestsController> _logger;

    public RepairRequestsController(
        CreateRepairRequestUseCase create,
        IRepairRequestRepository repository,
        IValidator<RepairRequestCreateDto> validator,
        IConfiguration config,
        ILogger<RepairRequestsController> logger)
    {
        _create = create;
        _repository = repository;
        _validator = validator;
        _config = config;
        _logger = logger;
    }

    /// <summary>Submit a new repair request.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RepairRequestCreateDto dto, CancellationToken ct)
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

        var result = await _create.ExecuteAsync(dto, ct);
        _logger.LogInformation("Repair request created: id={Id}", result.Id);
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }

    /// <summary>Upload files to an existing repair request (multipart/form-data).</summary>
    [HttpPost("{id:int}/files")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadFiles(int id, IFormFileCollection files, CancellationToken ct)
    {
        var request = await _repository.GetByIdAsync(id, ct);
        if (request is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Repair request not found" } });

        if (!files.Any())
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "No files provided" } });

        var uploadsRoot = _config["UPLOADS_ROOT"] ?? "/app/uploads";
        if (!int.TryParse(_config["MAX_UPLOAD_MB"], out var maxMb)) maxMb = 25;
        var maxBytes = (long)maxMb * 1024 * 1024;

        var repairDir = Path.Combine(uploadsRoot, "repairs", id.ToString());
        Directory.CreateDirectory(repairDir);

        var saved = new List<object>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            if (file.Length > maxBytes)
            {
                _logger.LogWarning("Upload rejected: file too large ({Size} bytes) for repair {Id}", file.Length, id);
                return StatusCode(413, new { error = new { code = "FILE_TOO_LARGE", message = $"File exceeds {maxMb} MB limit" } });
            }

            var mimeType = file.ContentType;
            if (!AllowedMimeTypes.Contains(mimeType))
            {
                _logger.LogWarning("Upload rejected: disallowed mime type '{Mime}' for repair {Id}", mimeType, id);
                return BadRequest(new { error = new { code = "INVALID_FILE_TYPE", message = $"File type '{mimeType}' is not allowed" } });
            }

            var ext = Path.GetExtension(file.FileName);
            var guidName = $"{Guid.NewGuid()}{ext}";
            var destPath = Path.GetFullPath(Path.Combine(repairDir, guidName));

            if (!destPath.StartsWith(Path.GetFullPath(uploadsRoot), StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = new { code = "INVALID_PATH", message = "Invalid file path" } });

            await using var stream = System.IO.File.Create(destPath);
            await file.CopyToAsync(stream, ct);

            var relativePath = $"/uploads/repairs/{id}/{guidName}";
            var repairFile = new RepairRequestFile
            {
                RepairRequestId = id,
                Path = relativePath,
                OriginalName = file.FileName,
                MimeType = mimeType,
                SizeBytes = file.Length
            };

            await _repository.AddFileAsync(repairFile, ct);
            saved.Add(new { path = relativePath, originalName = file.FileName });
        }

        return Ok(new { uploaded = saved });
    }
}
