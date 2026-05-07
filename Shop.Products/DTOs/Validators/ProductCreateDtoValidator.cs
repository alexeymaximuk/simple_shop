using FluentValidation;

namespace Shop.Products.DTOs.Validators;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(x => x.Name).ValidProductName();
        RuleFor(x => x.Description).ValidProductDescription();
        RuleFor(x => x.Price).ValidPrice();
    }
}