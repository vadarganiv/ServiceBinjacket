using ServisBinjaket.Application.Common;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Products.DTOs;
using ServisBinjaket.Domain.Entities;

namespace ServisBinjaket.Application.Products.UseCases;

public class CreateProductUseCase
{
    private readonly IProductRepository _repo;

    public CreateProductUseCase(IProductRepository repo) => _repo = repo;

    public async Task<(AdminProductDetailDto? Result, string? ConflictField)> ExecuteAsync(
        ProductCreateDto dto, CancellationToken ct = default)
    {
        var slugSq = string.IsNullOrWhiteSpace(dto.SlugSq)
            ? SlugHelper.Generate(dto.NameSq)
            : dto.SlugSq.Trim();
        var slugEn = string.IsNullOrWhiteSpace(dto.SlugEn)
            ? (dto.NameEn is not null ? SlugHelper.Generate(dto.NameEn) : null)
            : dto.SlugEn.Trim();

        if (await _repo.SlugExistsAsync(slugSq, slugEn, null, ct))
            return (null, "slug");

        var product = new Product
        {
            NameSq = dto.NameSq,
            NameEn = dto.NameEn,
            SlugSq = slugSq,
            SlugEn = slugEn,
            ShortDescriptionSq = dto.ShortDescriptionSq,
            ShortDescriptionEn = dto.ShortDescriptionEn,
            DescriptionSq = dto.DescriptionSq,
            DescriptionEn = dto.DescriptionEn,
            Price = dto.Price,
            Currency = dto.Currency,
            Condition = dto.Condition,
            StockQty = dto.StockQty,
            CategoryId = dto.CategoryId,
            WarrantyMonths = dto.WarrantyMonths,
            IsPublished = dto.IsPublished,
        };

        var created = await _repo.CreateAsync(product, ct);
        var full = await _repo.GetAdminByIdAsync(created.Id, ct);
        return (MapDetail(full!), null);
    }

    public static AdminProductDetailDto MapDetail(Product p) => new()
    {
        Id = p.Id,
        NameSq = p.NameSq,
        NameEn = p.NameEn,
        SlugSq = p.SlugSq,
        SlugEn = p.SlugEn,
        ShortDescriptionSq = p.ShortDescriptionSq,
        ShortDescriptionEn = p.ShortDescriptionEn,
        DescriptionSq = p.DescriptionSq,
        DescriptionEn = p.DescriptionEn,
        Price = p.Price,
        Currency = p.Currency,
        Condition = p.Condition.ToString(),
        StockQty = p.StockQty,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.NameSq ?? "",
        WarrantyMonths = p.WarrantyMonths,
        IsPublished = p.IsPublished,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Images = p.Images.OrderBy(i => i.SortOrder).Select(i => new AdminProductImageDto
        {
            Id = i.Id,
            Path = i.Path,
            AltSq = i.AltSq,
            AltEn = i.AltEn,
            SortOrder = i.SortOrder,
        }).ToList(),
    };
}
