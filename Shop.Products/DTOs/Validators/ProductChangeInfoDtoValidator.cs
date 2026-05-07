using FluentValidation;

namespace Shop.Products.DTOs.Validators;


public class ProductChangeInfoDtoValidator : AbstractValidator<ProductChangeInfoDto>
{
    public ProductChangeInfoDtoValidator()
    {
        RuleFor(x => x.Name).ValidProductName();
        RuleFor(x => x.Description).ValidProductDescription();
        RuleFor(x => x.Price).ValidPrice();
    }
}