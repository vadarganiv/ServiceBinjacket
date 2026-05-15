using ServisBinjaket.Application.Products.Queries;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Interfaces;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsAsync(GetProductsQuery query, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync(CancellationToken ct = default);

    // Admin
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetAdminListAsync(string? q, bool? isPublished, int page, int pageSize, CancellationToken ct = default);
    Task<Product?> GetAdminByIdAsync(int id, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task<bool> SetPublishedAsync(int id, bool isPublished, CancellationToken ct = default);
    Task<ProductImage> AddImageAsync(ProductImage image, CancellationToken ct = default);
    Task<bool> DeleteImageAsync(int imageId, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slugSq, string? slugEn, int? excludeId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductCategory>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
