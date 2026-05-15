using FluentValidation;
using ServisBinjaket.Application.Products.DTOs;

namespace ServisBinjaket.Application.Products.Validators;

public class ProductCreateValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateValidator()
    {
        RuleFor(x => x.NameSq).NotEmpty().MaximumLength(300);
        RuleFor(x => x.NameEn).MaximumLength(300).When(x => x.NameEn != null);
        RuleFor(x => x.SlugSq).MaximumLength(300).When(x => x.SlugSq != null);
        RuleFor(x => x.SlugEn).MaximumLength(300).When(x => x.SlugEn != null);
        RuleFor(x => x.ShortDescriptionSq).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.StockQty).GreaterThanOrEqualTo(0).When(x => x.StockQty.HasValue);
        RuleFor(x => x.WarrantyMonths).GreaterThan(0).When(x => x.WarrantyMonths.HasValue);
    }
}

public class ProductUpdateValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateValidator()
    {
        RuleFor(x => x.NameSq).NotEmpty().MaximumLength(300);
        RuleFor(x => x.NameEn).MaximumLength(300).When(x => x.NameEn != null);
        RuleFor(x => x.SlugSq).MaximumLength(300).When(x => x.SlugSq != null);
        RuleFor(x => x.SlugEn).MaximumLength(300).When(x => x.SlugEn != null);
        RuleFor(x => x.ShortDescriptionSq).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.StockQty).GreaterThanOrEqualTo(0).When(x => x.StockQty.HasValue);
        RuleFor(x => x.WarrantyMonths).GreaterThan(0).When(x => x.WarrantyMonths.HasValue);
    }
}
