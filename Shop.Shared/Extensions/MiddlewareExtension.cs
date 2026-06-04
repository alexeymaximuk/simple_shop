using Microsoft.AspNetCore.Builder;
using Shop.Shared.Middleware;

namespace Shop.Shared.Extensions;

public static class MiddlewareExtension
{
    public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionValidationMiddleware>();
    }
}