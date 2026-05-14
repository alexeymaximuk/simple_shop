using FluentValidation;
using Shop.Products.Application.DTOs;

namespace Shop.Products.Application.Validators;


public class ProductInfoDtoValidator : AbstractValidator<ProductInfoDto>
{
    public ProductInfoDtoValidator()
    {
        RuleFor(x => x.Name).ValidProductName();
        RuleFor(x => x.Description).ValidProductDescription();
        RuleFor(x => x.Price).ValidPrice();
    }
}