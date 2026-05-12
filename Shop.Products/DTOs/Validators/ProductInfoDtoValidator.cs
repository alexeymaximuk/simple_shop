using FluentValidation;

namespace Shop.Products.DTOs.Validators;


public class ProductInfoDtoValidator : AbstractValidator<ProductInfoDto>
{
    public ProductInfoDtoValidator()
    {
        RuleFor(x => x.Name).ValidProductName();
        RuleFor(x => x.Description).ValidProductDescription();
        RuleFor(x => x.Price).ValidPrice();
    }
}