using FluentValidation;
using ServisBinjaket.Application.Orders.DTOs;
using ServisBinjaket.Domain.Enums;

namespace ServisBinjaket.Application.Orders.Validators;

public class OrderCreateValidator : AbstractValidator<OrderCreateDto>
{
    private static readonly HashSet<PaymentMethod> AllowedPaymentMethods =
        [PaymentMethod.CashOnDelivery, PaymentMethod.CashInStore];

    public OrderCreateValidator()
    {
        RuleFor(x => x.Customer).NotNull().SetValidator(new OrderCustomerCreateValidator());
        RuleFor(x => x.DeliveryMethod).IsInEnum();
        RuleFor(x => x.PaymentMethod)
            .Must(m => AllowedPaymentMethods.Contains(m))
            .WithMessage("Only CashOnDelivery and CashInStore are accepted");
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item");
        RuleForEach(x => x.Items).SetValidator(new OrderItemCreateValidator());
        RuleFor(x => x.CustomerComment).MaximumLength(2000).When(x => x.CustomerComment != null);
        RuleFor(x => x.Consent).Equal(true).WithMessage("Consent is required");
    }
}

public class OrderCustomerCreateValidator : AbstractValidator<OrderCustomerCreateDto>
{
    private static readonly System.Text.RegularExpressions.Regex PhoneRegex =
        new(@"^\+?[0-9\s\-\(\)]{7,20}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public OrderCustomerCreateValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50)
            .Matches(PhoneRegex).WithMessage("Invalid phone number format");
        RuleFor(x => x.City).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address != null);
        RuleFor(x => x.WhatsAppPhone)
            .MaximumLength(50)
            .Matches(PhoneRegex).WithMessage("Invalid WhatsApp phone number format")
            .When(x => !string.IsNullOrEmpty(x.WhatsAppPhone));
    }
}

public class OrderItemCreateValidator : AbstractValidator<OrderItemCreateDto>
{
    public OrderItemCreateValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100);
    }
}
