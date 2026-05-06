using Shop.Users.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Users.Data;
using Shop.Users.DTOs.Auth;
using Shop.Users.Exceptions;
using Shop.Users.Models;

namespace Shop.Users.Services;

public partial class AuthService (
    UsersDbContext dbContext, 
    IPasswordHasher<User> passwordHasher, 
    JwtSettings jwtSettings, 
    IEmailService emailService
) : IAuthService
{
    public async Task<string> Login(LoginUserDto dto)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null) throw new NotFoundException("User with that email not found");
        if (!user.IsActive) throw new AccountDeactivatedException("Account is not active");
        if (!user.IsEmailConfirmed) throw new AuthorisationException("Email not confirmed");
        
        var passwordVerificationStatus = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (passwordVerificationStatus != PasswordVerificationResult.Success)
            throw new AuthorisationException("Password is incorrect");
        
        return GenerateJwtToken(user);
    }
}