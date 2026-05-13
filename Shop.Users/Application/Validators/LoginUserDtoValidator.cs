using FluentValidation;
using Shop.Users.Application.DTOs.Auth;

namespace Shop.Users.Application.DTOs.Validators;

public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDtoValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).ValidPassword();
    }
}