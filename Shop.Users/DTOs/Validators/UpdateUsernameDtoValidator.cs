using FluentValidation;

namespace Shop.Users.DTOs.Validators;

public class UpdateUsernameDtoValidator : AbstractValidator<UpdateUsernameDto>
{
    public UpdateUsernameDtoValidator()
    {
        RuleFor(x => x.Name).ValidUsername();
    }
    
}