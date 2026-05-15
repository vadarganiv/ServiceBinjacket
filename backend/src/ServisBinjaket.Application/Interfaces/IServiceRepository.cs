using ServisBinjaket.Application.Services.Queries;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Interfaces;

public interface IServiceRepository
{
    Task<IReadOnlyList<Service>> GetServicesAsync(GetServicesQuery query, CancellationToken ct = default);
    Task<Service?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceCategory>> GetCategoriesAsync(CancellationToken ct = default);
}
