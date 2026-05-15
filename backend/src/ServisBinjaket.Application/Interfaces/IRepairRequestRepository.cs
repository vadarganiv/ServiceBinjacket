using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Interfaces;

public interface IRepairRequestRepository
{
    Task<RepairRequest> CreateAsync(Customer customer, RepairRequest request, CancellationToken ct = default);
    Task<RepairRequest?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddFileAsync(RepairRequestFile file, CancellationToken ct = default);
    Task<(IReadOnlyList<RepairRequest> Items, int TotalCount)> GetAdminListAsync(
        RepairStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<RepairRequest?> GetAdminDetailAsync(int id, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(int id, RepairStatus status, CancellationToken ct = default);
    Task<bool> UpdateCommentAsync(int id, string? comment, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
