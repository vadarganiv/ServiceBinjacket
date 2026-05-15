using FluentValidation;
using ServisBinjaket.Application.Auth.DTOs;

namespace ServisBinjaket.Application.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(100);
    }
}
