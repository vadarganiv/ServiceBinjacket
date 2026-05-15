using ServisBinjaket.Application.Services.Queries;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Interfaces;

public interface IServiceRepository
{
    Task<IReadOnlyList<Service>> GetServicesAsync(GetServicesQuery query, CancellationToken ct = default);
    Task<Service?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceCategory>> GetCategoriesAsync(CancellationToken ct = default);

    // Admin
    Task<(IReadOnlyList<Service> Items, int TotalCount)> GetAdminListAsync(string? q, bool? isPublished, int page, int pageSize, CancellationToken ct = default);
    Task<Service?> GetAdminByIdAsync(int id, CancellationToken ct = default);
    Task<Service> CreateAsync(Service service, CancellationToken ct = default);
    Task UpdateAsync(Service service, CancellationToken ct = default);
    Task<bool> SetPublishedAsync(int id, bool isPublished, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slugSq, string? slugEn, int? excludeId, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceCategory>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
