using FluentValidation;

namespace Shop.Products.Application.Validators;

public static class ValidationRules
{
    public static IRuleBuilderOptions<T, string?> ValidProductName<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);
    }

    public static IRuleBuilderOptions<T, string> ValidProductDescription<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty()
            .MinimumLength(20)
            .MaximumLength(2000);
    }

    public static IRuleBuilderOptions<T, decimal> ValidPrice<T>(this IRuleBuilder<T, decimal> rule)
    {
        return rule
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);
    }
    
    public static IRuleBuilderOptions<T, decimal?> ValidPrice<T>(this IRuleBuilder<T, decimal?> rule)
    {
        return rule
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);
    }
}