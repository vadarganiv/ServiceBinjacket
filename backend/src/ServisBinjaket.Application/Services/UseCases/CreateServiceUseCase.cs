using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Services.DTOs;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Services.UseCases;

public class CreateServiceUseCase
{
    private readonly IServiceRepository _repo;

    public CreateServiceUseCase(IServiceRepository repo) => _repo = repo;

    public async Task<(AdminServiceDetailDto? Result, string? ConflictField)> ExecuteAsync(
        ServiceCreateDto dto, CancellationToken ct = default)
    {
        var slugSq = string.IsNullOrWhiteSpace(dto.SlugSq)
            ? SlugHelper.Generate(dto.NameSq)
            : dto.SlugSq.Trim();
        var slugEn = string.IsNullOrWhiteSpace(dto.SlugEn)
            ? (dto.NameEn is not null ? SlugHelper.Generate(dto.NameEn) : null)
            : dto.SlugEn.Trim();

        if (await _repo.SlugExistsAsync(slugSq, slugEn, null, ct))
            return (null, "slug");

        var service = new Service
        {
            NameSq = dto.NameSq,
            NameEn = dto.NameEn,
            SlugSq = slugSq,
            SlugEn = slugEn,
            ShortDescriptionSq = dto.ShortDescriptionSq,
            ShortDescriptionEn = dto.ShortDescriptionEn,
            DescriptionSq = dto.DescriptionSq,
            DescriptionEn = dto.DescriptionEn,
            PriceNoteSq = dto.PriceNoteSq,
            PriceNoteEn = dto.PriceNoteEn,
            CategoryId = dto.CategoryId,
            IsPublished = dto.IsPublished,
        };

        var created = await _repo.CreateAsync(service, ct);
        var full = await _repo.GetAdminByIdAsync(created.Id, ct);
        return (MapDetail(full!), null);
    }

    public static AdminServiceDetailDto MapDetail(Service s) => new()
    {
        Id = s.Id,
        NameSq = s.NameSq,
        NameEn = s.NameEn,
        SlugSq = s.SlugSq,
        SlugEn = s.SlugEn,
        ShortDescriptionSq = s.ShortDescriptionSq,
        ShortDescriptionEn = s.ShortDescriptionEn,
        DescriptionSq = s.DescriptionSq,
        DescriptionEn = s.DescriptionEn,
        PriceNoteSq = s.PriceNoteSq,
        PriceNoteEn = s.PriceNoteEn,
        CategoryId = s.CategoryId,
        CategoryName = s.Category?.NameSq ?? "",
        IsPublished = s.IsPublished,
    };
}
