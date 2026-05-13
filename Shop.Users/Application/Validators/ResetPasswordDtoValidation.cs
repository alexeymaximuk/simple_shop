using FluentValidation;
using Shop.Users.Application.DTOs.Auth;

namespace Shop.Users.Application.DTOs.Validators;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Password).ValidPassword();
    }
}