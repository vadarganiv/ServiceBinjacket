using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Products.DTOs;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Products.UseCases;

public class GetProductBySlugUseCase
{
    private readonly IProductRepository _repository;

    public GetProductBySlugUseCase(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDetailDto?> ExecuteAsync(string slug, string? locale, CancellationToken ct = default)
    {
        var resolvedLocale = LocalizationHelper.NormalizeLocale(locale);
        var product = await _repository.GetBySlugAsync(slug, ct);
        return product is null ? null : MapToDetail(product, resolvedLocale);
    }

    private static ProductDetailDto MapToDetail(Product p, string locale) =>
        new()
        {
            Id = p.Id,
            Slug = LocalizationHelper.Resolve(p.SlugSq, p.SlugEn, locale),
            Name = LocalizationHelper.Resolve(p.NameSq, p.NameEn, locale),
            ShortDescription = LocalizationHelper.Resolve(p.ShortDescriptionSq, p.ShortDescriptionEn, locale),
            Description = LocalizationHelper.Resolve(p.DescriptionSq, p.DescriptionEn, locale),
            Price = p.Price,
            Currency = p.Currency,
            Condition = p.Condition,
            StockQty = p.StockQty,
            WarrantyMonths = p.WarrantyMonths,
            InStock = p.StockQty == null || p.StockQty > 0,
            Category = p.Category is null ? null : new ProductCategoryDto
            {
                Id = p.Category.Id,
                Name = LocalizationHelper.Resolve(p.Category.NameSq, p.Category.NameEn, locale),
                Slug = LocalizationHelper.Resolve(p.Category.SlugSq, p.Category.SlugEn, locale),
                ParentId = p.Category.ParentId,
                SortOrder = p.Category.SortOrder
            },
            Images = p.Images
                .OrderBy(i => i.SortOrder)
                .Select(i => new ProductImageDto
                {
                    Path = i.Path,
                    Alt = LocalizationHelper.Resolve(i.AltSq, i.AltEn, locale)
                })
                .ToList()
        };
}
