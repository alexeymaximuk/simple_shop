using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Shop.Shared.Exceptions;
using Shop.Shared.Settings;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;
using Shop.Users.Domain.Models;

namespace Shop.Users.Application.Services;

public class LoginService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    JwtSettings jwtSettings
) : ILoginService
{
    public async Task<string> Login(LoginUserDto dto)
    {
        var user = await userRepository.GetByEmailAsync(dto.Email);

        if (user == null) throw new NotFoundException("User with that email not found");
        if (!user.IsActive) throw new AccountDeactivatedException("Account is not active");
        if (!user.IsEmailConfirmed) throw new AuthorisationException("Email not confirmed");

        var passwordVerificationStatus = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (passwordVerificationStatus != PasswordVerificationResult.Success)
            throw new AuthorisationException("Password is incorrect");

        return GenerateJwtToken(user);
    }


    private string GenerateJwtToken(User user)
    {
        var claims = new []
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(jwtSettings.TokenDuration),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}