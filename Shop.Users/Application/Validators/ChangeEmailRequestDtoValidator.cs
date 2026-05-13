using FluentValidation;
using Shop.Users.Application.DTOs.Auth;

namespace Shop.Users.Application.DTOs.Validators;

public class ChangeEmailRequestDtoValidator : AbstractValidator<ChangeEmailRequestDto>
{
    public ChangeEmailRequestDtoValidator()
    {
        RuleFor(x => x.NewEmail).ValidEmail();
    }
    
}