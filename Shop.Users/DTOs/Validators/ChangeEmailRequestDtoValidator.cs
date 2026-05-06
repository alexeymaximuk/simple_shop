using FluentValidation;
using Shop.Users.DTOs.Auth;

namespace Shop.Users.DTOs.Validators;

public class ChangeEmailRequestDtoValidator : AbstractValidator<ChangeEmailRequestDto>
{
    public ChangeEmailRequestDtoValidator()
    {
        RuleFor(x => x.NewEmail).ValidEmail();
    }
    
}