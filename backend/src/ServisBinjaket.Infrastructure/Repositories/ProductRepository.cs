using Microsoft.EntityFrameworkCore;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Products.Queries;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Infrastructure.Persistence;

namespace ServisBinjaket.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

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

        if (query.MinPrice.HasValue)
            q = q.Where(p => p.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            q = q.Where(p => p.Price <= query.MaxPrice.Value);

        if (query.Condition.HasValue)
            q = q.Where(p => p.Condition == query.Condition.Value);

        if (query.InStock.HasValue)
        {
            if (query.InStock.Value)
                q = q.Where(p => p.StockQty == null || p.StockQty > 0);
            else
                q = q.Where(p => p.StockQty == 0);
        }

        q = query.Sort switch
        {
            "price-asc"  => q.OrderBy(p => p.Price),
            "price-desc" => q.OrderByDescending(p => p.Price),
            "name-asc"   => q.OrderBy(p => p.NameSq),
            _            => q.OrderByDescending(p => p.CreatedAt)
        };

        var total = await q.CountAsync(ct);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

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
}
