using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Products.DTOs;

namespace ServisBinjaket.Application.Products.UseCases;

public class UpdateProductUseCase
{
    private readonly IProductRepository _repo;

    public UpdateProductUseCase(IProductRepository repo) => _repo = repo;

    public async Task<(AdminProductDetailDto? Result, string? Error)> ExecuteAsync(
        int id, ProductUpdateDto dto, CancellationToken ct = default)
    {
        var product = await _repo.GetAdminByIdAsync(id, ct);
        if (product is null) return (null, "not_found");

        var slugSq = string.IsNullOrWhiteSpace(dto.SlugSq)
            ? SlugHelper.Generate(dto.NameSq)
            : dto.SlugSq.Trim();
        var slugEn = string.IsNullOrWhiteSpace(dto.SlugEn)
            ? (dto.NameEn is not null ? SlugHelper.Generate(dto.NameEn) : null)
            : dto.SlugEn.Trim();

        if (await _repo.SlugExistsAsync(slugSq, slugEn, id, ct))
            return (null, "slug");

        product.NameSq = dto.NameSq;
        product.NameEn = dto.NameEn;
        product.SlugSq = slugSq;
        product.SlugEn = slugEn;
        product.ShortDescriptionSq = dto.ShortDescriptionSq;
        product.ShortDescriptionEn = dto.ShortDescriptionEn;
        product.DescriptionSq = dto.DescriptionSq;
        product.DescriptionEn = dto.DescriptionEn;
        product.Price = dto.Price;
        product.Currency = dto.Currency;
        product.Condition = dto.Condition;
        product.StockQty = dto.StockQty;
        product.CategoryId = dto.CategoryId;
        product.WarrantyMonths = dto.WarrantyMonths;

        await _repo.UpdateAsync(product, ct);

        var updated = await _repo.GetAdminByIdAsync(id, ct);
        return (CreateProductUseCase.MapDetail(updated!), null);
    }
}
