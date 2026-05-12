using FluentValidation;
using Shop.Users.DTOs.Auth;

namespace Shop.Users.DTOs.Validators;

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
    }
}