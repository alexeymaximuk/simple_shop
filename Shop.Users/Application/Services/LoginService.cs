using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Shop.Shared.Constants;
using Shop.Shared.Exceptions;
using Shop.Shared.Interfaces;
using Shop.Shared.Settings;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;
using Shop.Users.Domain.Models;

namespace Shop.Users.Application.Services;

public class LoginService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    JwtSettings jwtSettings,
    IRedisSessionService redisSessionService
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
        var jtiToken = Guid.NewGuid();
        var token = GenerateJwtToken(user, jtiToken);
        await redisSessionService.SetSessionAsync(jtiToken, user.Id.ToString());
        return token;
    }


    private string GenerateJwtToken(User user, Guid jtiToken)
    {
        var claims = new[]
        {
            new Claim(AuthConstants.ClaimNames.Sub, user.Id.ToString()),
            new Claim(AuthConstants.ClaimNames.Jti, jtiToken.ToString()),
            new Claim(AuthConstants.ClaimNames.Name, user.Name),
            new Claim(AuthConstants.ClaimNames.Email, user.Email),
            new Claim(AuthConstants.ClaimNames.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(jwtSettings.TokenDuration),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}