using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Services.DTOs;

namespace ServisBinjaket.Application.Services.UseCases;

public class UpdateServiceUseCase
{
    private readonly IServiceRepository _repo;

    public UpdateServiceUseCase(IServiceRepository repo) => _repo = repo;

    public async Task<(AdminServiceDetailDto? Result, string? Error)> ExecuteAsync(
        int id, ServiceUpdateDto dto, CancellationToken ct = default)
    {
        var service = await _repo.GetAdminByIdAsync(id, ct);
        if (service is null) return (null, "not_found");

        var slugSq = string.IsNullOrWhiteSpace(dto.SlugSq)
            ? SlugHelper.Generate(dto.NameSq)
            : dto.SlugSq.Trim();
        var slugEn = string.IsNullOrWhiteSpace(dto.SlugEn)
            ? (dto.NameEn is not null ? SlugHelper.Generate(dto.NameEn) : null)
            : dto.SlugEn.Trim();

        if (await _repo.SlugExistsAsync(slugSq, slugEn, id, ct))
            return (null, "slug");

        service.NameSq = dto.NameSq;
        service.NameEn = dto.NameEn;
        service.SlugSq = slugSq;
        service.SlugEn = slugEn;
        service.ShortDescriptionSq = dto.ShortDescriptionSq;
        service.ShortDescriptionEn = dto.ShortDescriptionEn;
        service.DescriptionSq = dto.DescriptionSq;
        service.DescriptionEn = dto.DescriptionEn;
        service.PriceNoteSq = dto.PriceNoteSq;
        service.PriceNoteEn = dto.PriceNoteEn;
        service.CategoryId = dto.CategoryId;

        await _repo.UpdateAsync(service, ct);

        var updated = await _repo.GetAdminByIdAsync(id, ct);
        return (CreateServiceUseCase.MapDetail(updated!), null);
    }
}
