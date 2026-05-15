using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.RepairRequests.DTOs;
using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/repair-requests")]
public class AdminRepairRequestsController : ControllerBase
{
    private readonly IRepairRequestRepository _repository;

    public AdminRepairRequestsController(IRepairRequestRepository repository)
    {
        _repository = repository;
    }

    /// <summary>List all repair requests (admin). TODO: requires auth (TASK-012).</summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] RepairStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var (items, total) = await _repository.GetAdminListAsync(status, page, pageSize, ct);

        var dtos = items.Select(r => new RepairRequestAdminListItemDto
        {
            Id = r.Id,
            CustomerName = r.Customer.FullName,
            CustomerPhone = r.Customer.Phone,
            DeviceType = r.DeviceType,
            Brand = r.Brand,
            Model = r.Model,
            Status = r.Status.ToString(),
            FileCount = r.Files.Count,
            CreatedAt = r.CreatedAt
        }).ToList();

        return Ok(new { items = dtos, totalCount = total, page, pageSize });
    }

    /// <summary>Get repair request detail (admin). TODO: requires auth (TASK-012).</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var r = await _repository.GetAdminDetailAsync(id, ct);
        if (r is null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Repair request not found" } });

        return Ok(new
        {
            id = r.Id,
            status = r.Status.ToString(),
            adminComment = r.AdminComment,
            createdAt = r.CreatedAt,
            updatedAt = r.UpdatedAt,
            customer = new
            {
                fullName = r.Customer.FullName,
                phone = r.Customer.Phone,
                whatsAppPhone = r.Customer.WhatsAppPhone,
                city = r.Customer.City
            },
            service = r.Service is null ? null : new { id = r.Service.Id, name = r.Service.NameSq },
            deviceType = r.DeviceType,
            brand = r.Brand,
            model = r.Model,
            problemDescription = r.ProblemDescription,
            preferredDeliveryMethod = r.PreferredDeliveryMethod.ToString(),
            customerComment = r.CustomerComment,
            files = r.Files.Select(f => new
            {
                id = f.Id,
                path = f.Path,
                originalName = f.OriginalName,
                mimeType = f.MimeType,
                sizeBytes = f.SizeBytes,
                uploadedAt = f.UploadedAt
            })
        });
    }
}
