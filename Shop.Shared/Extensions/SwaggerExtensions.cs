using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Shop.Shared.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
            {
                var securityDefinitionScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                };
                options.AddSecurityDefinition("Bearer", securityDefinitionScheme);

                var reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                };
                var securityRequirementsScheme = new OpenApiSecurityScheme {Reference = reference};
                var requirement = new OpenApiSecurityRequirement {{securityRequirementsScheme, []}};
                options.AddSecurityRequirement(requirement);
            }
        );

        return services;
    }
}