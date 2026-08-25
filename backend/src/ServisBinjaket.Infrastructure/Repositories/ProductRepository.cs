using Microsoft.EntityFrameworkCore;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Products.Queries;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Infrastructure.Persistence;

namespace ServisBinjaket.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsAsync(
        GetProductsQuery query, CancellationToken ct = default)
    {
        var q = _db.Products
            .Include(p => p.Images)
            .Where(p => p.IsPublished)
            .AsQueryable();

        if (query.CategoryId.HasValue)
            q = q.Where(p => p.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var search = query.Q.ToLower();
            q = q.Where(p =>
                p.NameSq.ToLower().Contains(search) ||
                (p.NameEn != null && p.NameEn.ToLower().Contains(search)) ||
                p.ShortDescriptionSq.ToLower().Contains(search) ||
                (p.ShortDescriptionEn != null && p.ShortDescriptionEn.ToLower().Contains(search)));
        }

        if (query.MinPrice.HasValue) q = q.Where(p => p.Price >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue) q = q.Where(p => p.Price <= query.MaxPrice.Value);
        if (query.Condition.HasValue) q = q.Where(p => p.Condition == query.Condition.Value);

        if (query.InStock.HasValue)
        {
            if (query.InStock.Value)
                q = q.Where(p => p.StockQty == null || p.StockQty > 0);
            else
                q = q.Where(p => p.StockQty == 0);
        }

        q = query.Sort switch
        {
            "price-asc" => q.OrderBy(p => p.Price),
            "price-desc" => q.OrderByDescending(p => p.Price),
            "name-asc" => q.OrderBy(p => p.NameSq),
            _ => q.OrderByDescending(p => p.CreatedAt)
        };

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Where(p => p.IsPublished && (p.SlugSq == slug || p.SlugEn == slug))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await _db.Products
            .Where(p => p.IsPublished && idList.Contains(p.Id))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await _db.ProductCategories
            .Where(c => c.IsPublished)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);
    }

    // ── Admin ──────────────────────────────────────────────────────────────────

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetAdminListAsync(
        string? q, bool? isPublished, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.ToLower();
            query = query.Where(p =>
                p.NameSq.ToLower().Contains(search) ||
                (p.NameEn != null && p.NameEn.ToLower().Contains(search)));
        }

        if (isPublished.HasValue)
            query = query.Where(p => p.IsPublished == isPublished.Value);

        var total = await query.CountAsync(ct);
        var clampedPage = Math.Max(1, page);
        var clampedSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((clampedPage - 1) * clampedSize)
            .Take(clampedSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Product?> GetAdminByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> SetPublishedAsync(int id, bool isPublished, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync([id], ct);
        if (product is null) return false;
        product.IsPublished = isPublished;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ProductImage> AddImageAsync(ProductImage image, CancellationToken ct = default)
    {
        var maxOrder = await _db.ProductImages
            .Where(i => i.ProductId == image.ProductId)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(ct) ?? -1;
        image.SortOrder = maxOrder + 1;

        _db.ProductImages.Add(image);
        await _db.SaveChangesAsync(ct);
        return image;
    }

    public async Task<string?> DeleteImageAsync(int productId, int imageId, CancellationToken ct = default)
    {
        var image = await _db.ProductImages.SingleOrDefaultAsync(
            candidate => candidate.Id == imageId && candidate.ProductId == productId,
            ct);
        if (image is null) return null;

        var storedPath = image.Path;
        _db.ProductImages.Remove(image);
        await _db.SaveChangesAsync(ct);
        return storedPath;
    }

    public async Task<bool> SlugExistsAsync(string slugSq, string? slugEn, int? excludeId, CancellationToken ct = default)
    {
        var q = _db.Products.AsQueryable();
        if (excludeId.HasValue)
            q = q.Where(p => p.Id != excludeId.Value);

        return await q.AnyAsync(p =>
            p.SlugSq == slugSq ||
            (slugEn != null && (p.SlugSq == slugEn || p.SlugEn == slugEn)) ||
            (p.SlugEn != null && p.SlugEn == slugSq), ct);
    }

    public async Task<IReadOnlyList<ProductCategory>> GetAllCategoriesAsync(CancellationToken ct = default)
    {
        return await _db.ProductCategories
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(CancellationToken ct = default) =>
        _db.Products.CountAsync(ct);
}
