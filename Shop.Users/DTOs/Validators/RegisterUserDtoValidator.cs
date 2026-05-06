using FluentValidation;
using Shop.Users.DTOs.Auth;

namespace Shop.Users.DTOs.Validators;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Name).ValidUsername();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).ValidPassword();
    }
    
}