using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Services.DTOs;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Services.UseCases;

public class GetServiceBySlugUseCase
{
    private readonly IServiceRepository _repository;

    public GetServiceBySlugUseCase(IServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceDetailDto?> ExecuteAsync(string slug, string? locale, CancellationToken ct = default)
    {
        var resolvedLocale = LocalizationHelper.NormalizeLocale(locale);
        var service = await _repository.GetBySlugAsync(slug, ct);
        return service is null ? null : MapToDetail(service, resolvedLocale);
    }

    private static ServiceDetailDto MapToDetail(Service s, string locale)
    {
        var priceNote = LocalizationHelper.Resolve(s.PriceNoteSq, s.PriceNoteEn, locale);
        return new ServiceDetailDto
        {
            Id = s.Id,
            Slug = LocalizationHelper.Resolve(s.SlugSq, s.SlugEn, locale),
            Name = LocalizationHelper.Resolve(s.NameSq, s.NameEn, locale),
            ShortDescription = LocalizationHelper.Resolve(s.ShortDescriptionSq, s.ShortDescriptionEn, locale),
            Description = LocalizationHelper.Resolve(s.DescriptionSq, s.DescriptionEn, locale),
            PriceNote = string.IsNullOrWhiteSpace(priceNote) ? null : priceNote,
            Category = s.Category is null ? null : new ServiceCategoryDto
            {
                Id = s.Category.Id,
                Name = LocalizationHelper.Resolve(s.Category.NameSq, s.Category.NameEn, locale),
                Slug = LocalizationHelper.Resolve(s.Category.SlugSq, s.Category.SlugEn, locale),
                SortOrder = s.Category.SortOrder
            }
        };
    }
}
