using FluentValidation;

namespace Shop.Users.DTOs;

public class UpdateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(5);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
    
}