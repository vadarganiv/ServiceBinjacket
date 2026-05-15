using FluentValidation;
using ServisBinjaket.Application.RepairRequests.DTOs;

namespace ServisBinjaket.Application.RepairRequests.Validators;

public class RepairRequestCreateValidator : AbstractValidator<RepairRequestCreateDto>
{
    public RepairRequestCreateValidator()
    {
        RuleFor(x => x.Customer).NotNull().SetValidator(new CustomerCreateValidator());
        RuleFor(x => x.DeviceType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).MaximumLength(200).When(x => x.Brand != null);
        RuleFor(x => x.Model).MaximumLength(200).When(x => x.Model != null);
        RuleFor(x => x.ProblemDescription).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.PreferredDeliveryMethod).IsInEnum();
        RuleFor(x => x.CustomerComment).MaximumLength(2000).When(x => x.CustomerComment != null);
        RuleFor(x => x.Consent).Equal(true).WithMessage("Consent is required");
    }
}

public class CustomerCreateValidator : AbstractValidator<CustomerCreateDto>
{
    private static readonly System.Text.RegularExpressions.Regex PhoneRegex =
        new(@"^\+?[0-9\s\-\(\)]{7,20}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public CustomerCreateValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50)
            .Matches(PhoneRegex).WithMessage("Invalid phone number format");
        RuleFor(x => x.City).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WhatsAppPhone)
            .MaximumLength(50)
            .Matches(PhoneRegex).WithMessage("Invalid WhatsApp phone number format")
            .When(x => !string.IsNullOrEmpty(x.WhatsAppPhone));
    }
}
