using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.RepairRequests.DTOs;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.RepairRequests.UseCases;

public class CreateRepairRequestUseCase
{
    private readonly IRepairRequestRepository _repository;

    public CreateRepairRequestUseCase(IRepairRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<RepairRequestResponseDto> ExecuteAsync(RepairRequestCreateDto dto, CancellationToken ct = default)
    {
        var customer = new Customer
        {
            FullName = dto.Customer.FullName,
            Phone = dto.Customer.Phone,
            WhatsAppPhone = dto.Customer.WhatsAppPhone,
            City = dto.Customer.City
        };

        var request = new RepairRequest
        {
            ServiceId = dto.ServiceId,
            DeviceType = dto.DeviceType,
            Brand = dto.Brand,
            Model = dto.Model,
            ProblemDescription = dto.ProblemDescription,
            PreferredDeliveryMethod = dto.PreferredDeliveryMethod,
            CustomerComment = dto.CustomerComment,
            Status = RepairStatus.New
        };

        var saved = await _repository.CreateAsync(customer, request, ct);

        return new RepairRequestResponseDto
        {
            Id = saved.Id,
            Status = saved.Status,
            CreatedAt = saved.CreatedAt
        };
    }
}
