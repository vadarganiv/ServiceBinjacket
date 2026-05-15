using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Services.DTOs;

namespace ServisBinjaket.Application.Services.UseCases;

public class GetServiceCategoriesUseCase
{
    private readonly IServiceRepository _repository;

    public GetServiceCategoriesUseCase(IServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ServiceCategoryDto>> ExecuteAsync(string? locale, CancellationToken ct = default)
    {
        var resolvedLocale = LocalizationHelper.NormalizeLocale(locale);
        var categories = await _repository.GetCategoriesAsync(ct);

        return categories
            .Select(c => new ServiceCategoryDto
            {
                Id = c.Id,
                Name = LocalizationHelper.Resolve(c.NameSq, c.NameEn, resolvedLocale),
                Slug = LocalizationHelper.Resolve(c.SlugSq, c.SlugEn, resolvedLocale),
                SortOrder = c.SortOrder
            })
            .ToList();
    }
}
