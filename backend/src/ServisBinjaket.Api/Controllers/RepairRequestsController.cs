using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServisBinjaket.Api.Uploads;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.RepairRequests.DTOs;
using ServisBinjaket.Application.RepairRequests.UseCases;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Api.Controllers;

[ApiController]
[Route("api/v1/repair-requests")]
public class RepairRequestsController : ControllerBase
{
    private const long MaxTransportBytes = 32L * 1024 * 1024;

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
    [RequestFormLimits(MultipartBodyLengthLimit = MaxTransportBytes)]
    [RequestSizeLimit(MaxTransportBytes)]
    [EnableRateLimiting("repair-upload")]
    public async Task<IActionResult> UploadFiles(
        int id,
        [FromForm] IFormFileCollection files,
        CancellationToken ct)
    {
        var request = await _repository.GetByIdAsync(id, ct);
        if (request is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Repair request not found" } });

        var candidates = files.Where(file => file.Length > 0).ToList();
        if (candidates.Count == 0)
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "No files provided" } });

        var maxFileMb = ReadBoundedInt("MAX_UPLOAD_MB", defaultValue: 25, minimum: 1, maximum: 25);
        var maxFiles = ReadBoundedInt("MAX_UPLOAD_FILES", defaultValue: 4, minimum: 1, maximum: 10);
        var maxTotalMb = ReadBoundedInt("MAX_UPLOAD_TOTAL_MB", defaultValue: 30, minimum: 1, maximum: 30);
        var maxFileBytes = (long)maxFileMb * 1024 * 1024;
        var maxTotalBytes = (long)maxTotalMb * 1024 * 1024;

        if (candidates.Count > maxFiles)
        {
            return BadRequest(new
            {
                error = new
                {
                    code = "TOO_MANY_FILES",
                    message = $"A maximum of {maxFiles} files can be uploaded at once"
                }
            });
        }

        long totalBytes = 0;
        var validatedFiles = new List<(IFormFile File, ValidatedUpload Metadata)>();

        foreach (var file in candidates)
        {
            if (file.Length > maxFileBytes)
            {
                _logger.LogWarning("Upload rejected: file too large ({Size} bytes) for repair {Id}", file.Length, id);
                return StatusCode(413, new
                {
                    error = new
                    {
                        code = "FILE_TOO_LARGE",
                        message = $"A file exceeds the {maxFileMb} MB per-file limit"
                    }
                });
            }

            if (totalBytes > maxTotalBytes - file.Length)
            {
                _logger.LogWarning("Upload rejected: total size limit exceeded for repair {Id}", id);
                return StatusCode(413, new
                {
                    error = new
                    {
                        code = "UPLOAD_TOO_LARGE",
                        message = $"Combined files exceed the {maxTotalMb} MB request limit"
                    }
                });
            }

            totalBytes += file.Length;

            var validation = await UploadFileValidator.ValidateAsync(
                file,
                UploadFileScope.RepairAttachment,
                ct);
            if (!validation.IsValid)
            {
                _logger.LogWarning(
                    "Upload rejected: {Code} for repair {Id}",
                    validation.ErrorCode,
                    id);
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
        var repairDir = Path.Combine(uploadsRoot, "repairs", id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Directory.CreateDirectory(repairDir);

        var saved = new List<object>();
        foreach (var (file, metadata) in validatedFiles)
        {
            var guidName = $"{Guid.NewGuid():N}{metadata.StorageExtension}";
            var destPath = Path.Combine(repairDir, guidName);

            var relativePath = $"/uploads/repairs/{id}/{guidName}";
            var repairFile = new RepairRequestFile
            {
                RepairRequestId = id,
                Path = relativePath,
                OriginalName = metadata.OriginalFileName,
                MimeType = metadata.ContentType,
                SizeBytes = file.Length
            };

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

                await _repository.AddFileAsync(repairFile, ct);
            }
            catch
            {
                TryDeleteFile(destPath);
                throw;
            }

            saved.Add(new { path = relativePath, originalName = metadata.OriginalFileName });
        }

        return Ok(new { uploaded = saved });
    }

    private int ReadBoundedInt(string key, int defaultValue, int minimum, int maximum)
    {
        return int.TryParse(_config[key], out var parsed)
            ? Math.Clamp(parsed, minimum, maximum)
            : defaultValue;
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
