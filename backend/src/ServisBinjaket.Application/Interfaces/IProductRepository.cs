using ServisBinjaket.Application.Products.Queries;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Interfaces;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsAsync(GetProductsQuery query, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync(CancellationToken ct = default);
}
