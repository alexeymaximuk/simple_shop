using Shop.Users.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Users.Data;
using Shop.Users.DTOs;
using Shop.Users.Exceptions;
using Shop.Users.Models;

namespace Shop.Users.Services;

public partial class AuthService(UsersDbContext dbContext, PasswordHasher<User> passwordHasher, JwtSettings jwtSettings) : IAuthService
{
    public Task ResetPasswordAsync(Guid id)
    {
        throw new  NotImplementedException();
    }
    
    public async Task<string> Login(LoginUserDto dto)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null) throw new NotFoundException("User with that email not found");
        
        var passwordVerificationStatus = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (passwordVerificationStatus != PasswordVerificationResult.Success)
            throw new AuthorisationException("Password is incorrect");
        
        
        return GenerateToken(user);
    }
}