using FluentValidation;
using ServisBinjaket.Application.Services.DTOs;

namespace ServisBinjaket.Application.Services.Validators;

public class ServiceCreateValidator : AbstractValidator<ServiceCreateDto>
{
    public ServiceCreateValidator()
    {
        RuleFor(x => x.NameSq).NotEmpty().MaximumLength(300);
        RuleFor(x => x.NameEn).MaximumLength(300).When(x => x.NameEn != null);
        RuleFor(x => x.SlugSq).MaximumLength(300).When(x => x.SlugSq != null);
        RuleFor(x => x.SlugEn).MaximumLength(300).When(x => x.SlugEn != null);
        RuleFor(x => x.ShortDescriptionSq).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}

public class ServiceUpdateValidator : AbstractValidator<ServiceUpdateDto>
{
    public ServiceUpdateValidator()
    {
        RuleFor(x => x.NameSq).NotEmpty().MaximumLength(300);
        RuleFor(x => x.NameEn).MaximumLength(300).When(x => x.NameEn != null);
        RuleFor(x => x.SlugSq).MaximumLength(300).When(x => x.SlugSq != null);
        RuleFor(x => x.SlugEn).MaximumLength(300).When(x => x.SlugEn != null);
        RuleFor(x => x.ShortDescriptionSq).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}
