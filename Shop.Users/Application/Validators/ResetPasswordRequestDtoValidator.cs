using FluentValidation;
using Shop.Users.Application.DTOs.Auth;

namespace Shop.Users.Application.Validators;

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
    }
}