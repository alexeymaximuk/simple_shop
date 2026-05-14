using FluentValidation;
using Shop.Products.Application.DTOs;

namespace Shop.Products.Application.Validators;

public class ProductFilterDtoValidator : AbstractValidator<ProductFilterDto>
{
    public ProductFilterDtoValidator()
    {
        RuleFor(x => x.Name).ValidProductName().When(x => x.Name != null);
        RuleFor(x => x.MinPrice).ValidPrice().When(x => x.MinPrice != null);
        RuleFor(x => x.MaxPrice).ValidPrice().When(x => x.MaxPrice != null);
        RuleFor(x => x)
            .Must(x => x.MinPrice <= x.MaxPrice)
            .When(x => x.MinPrice != null && x.MaxPrice != null)
            .WithMessage("MinPrice cannot be greater than MaxPrice");
    }
}