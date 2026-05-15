using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Services.DTOs;
using ServisBinjaket.Application.Services.Queries;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Services.UseCases;

public class GetServicesUseCase
{
    private readonly IServiceRepository _repository;

    public GetServicesUseCase(IServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ServiceListItemDto>> ExecuteAsync(GetServicesQuery query, CancellationToken ct = default)
    {
        var locale = LocalizationHelper.NormalizeLocale(query.Locale);
        var normalized = query with { Locale = locale };

        var services = await _repository.GetServicesAsync(normalized, ct);
        return services.Select(s => MapToListItem(s, locale)).ToList();
    }

    internal static ServiceListItemDto MapToListItem(Service s, string locale)
    {
        var priceNote = LocalizationHelper.Resolve(s.PriceNoteSq, s.PriceNoteEn, locale);
        return new ServiceListItemDto
        {
            Id = s.Id,
            Slug = LocalizationHelper.Resolve(s.SlugSq, s.SlugEn, locale),
            Name = LocalizationHelper.Resolve(s.NameSq, s.NameEn, locale),
            ShortDescription = LocalizationHelper.Resolve(s.ShortDescriptionSq, s.ShortDescriptionEn, locale),
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
