using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Products.DTOs;

namespace ServisBinjaket.Application.Products.UseCases;

public class GetProductCategoriesUseCase
{
    private readonly IProductRepository _repository;

    public GetProductCategoriesUseCase(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProductCategoryDto>> ExecuteAsync(string? locale, CancellationToken ct = default)
    {
        var resolvedLocale = LocalizationHelper.NormalizeLocale(locale);
        var categories = await _repository.GetCategoriesAsync(ct);

        return categories
            .Select(c => new ProductCategoryDto
            {
                Id = c.Id,
                Name = LocalizationHelper.Resolve(c.NameSq, c.NameEn, resolvedLocale),
                Slug = LocalizationHelper.Resolve(c.SlugSq, c.SlugEn, resolvedLocale),
                ParentId = c.ParentId,
                SortOrder = c.SortOrder
            })
            .ToList();
    }
}
