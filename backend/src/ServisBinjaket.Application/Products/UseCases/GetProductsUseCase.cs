using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Products.DTOs;
using ServisBinjaket.Application.Products.Queries;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Products.UseCases;

public class GetProductsUseCase
{
    private readonly IProductRepository _repository;

    public GetProductsUseCase(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ProductListItemDto>> ExecuteAsync(GetProductsQuery query, CancellationToken ct = default)
    {
        var locale = LocalizationHelper.NormalizeLocale(query.Locale);
        var normalized = query with { Locale = locale };

        var (items, total) = await _repository.GetProductsAsync(normalized, ct);

        return new PagedResult<ProductListItemDto>
        {
            Items = items.Select(p => MapToListItem(p, locale)).ToList(),
            TotalCount = total,
            Page = normalized.Page,
            PageSize = normalized.PageSize
        };
    }

    private static ProductListItemDto MapToListItem(Product p, string locale)
    {
        var firstImage = p.Images.OrderBy(i => i.SortOrder).FirstOrDefault();
        return new ProductListItemDto
        {
            Id = p.Id,
            Slug = LocalizationHelper.Resolve(p.SlugSq, p.SlugEn, locale),
            Name = LocalizationHelper.Resolve(p.NameSq, p.NameEn, locale),
            ShortDescription = LocalizationHelper.Resolve(p.ShortDescriptionSq, p.ShortDescriptionEn, locale),
            Price = p.Price,
            Currency = p.Currency,
            Condition = p.Condition,
            InStock = p.StockQty == null || p.StockQty > 0,
            Image = firstImage is null ? null : new ProductImageDto
            {
                Path = firstImage.Path,
                Alt = LocalizationHelper.Resolve(firstImage.AltSq, firstImage.AltEn, locale)
            }
        };
    }
}
