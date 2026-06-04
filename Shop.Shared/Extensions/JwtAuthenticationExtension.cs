using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shop.Shared.Constants;
using Shop.Shared.Settings;

namespace Shop.Shared.Extensions;

public static class JwtAuthenticationExtension
{
    public static AuthenticationBuilder AddJwtAuth(this IServiceCollection services, JwtSettings jwtSettings)
    {
        return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                        NameClaimType = AuthConstants.ClaimNames.Sub,
                        RoleClaimType = AuthConstants.ClaimNames.Role,
                    };
                }
            );
    }
}