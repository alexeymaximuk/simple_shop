using FluentValidation;
using Shop.Users.Application.DTOs.Auth;

namespace Shop.Users.Application.Validators;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Name).ValidUsername();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).ValidPassword();
    }
    
}