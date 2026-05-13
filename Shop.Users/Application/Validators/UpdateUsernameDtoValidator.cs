using FluentValidation;
using Shop.Users.Application.DTOs.Users;

namespace Shop.Users.Application.DTOs.Validators;

public class UpdateUsernameDtoValidator : AbstractValidator<UpdateUsernameDto>
{
    public UpdateUsernameDtoValidator()
    {
        RuleFor(x => x.Name).ValidUsername();
    }
    
}