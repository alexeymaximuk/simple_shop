using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shop.Shared.Constants;

namespace Shop.Products.Tests.Integration;

public static class TestJwt
{
    public static string Generate(IConfiguration config, Guid userId, string role = "User")
    {
        var claims = new[]
        {
            new Claim(AuthConstants.ClaimNames.Sub, userId.ToString()),
            new Claim(AuthConstants.ClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(AuthConstants.ClaimNames.Name, "TestUser"),
            new Claim(AuthConstants.ClaimNames.Email, "test@test.com"),
            new Claim(AuthConstants.ClaimNames.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["JwtSettings:Issuer"],
            audience: config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
