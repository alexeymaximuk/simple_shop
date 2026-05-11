using FluentValidation;
using Shop.Users.DTOs.Auth;

namespace Shop.Users.DTOs.Validators;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Password).ValidPassword();
    }
}