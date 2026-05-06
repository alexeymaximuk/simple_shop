using Microsoft.EntityFrameworkCore;
using Shop.Users.DTOs.Auth;
using Shop.Users.Exceptions;

namespace Shop.Users.Services;

public partial class AuthService
{
    public async Task RegisterAsync(RegisterUserDto dto)
    {
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (existingUser != null) throw new DuplicateMailException("Mail already registered");

        var newUser = MapToEntity(dto);

        var hashedPassword = passwordHasher.HashPassword(newUser, dto.Password);
        newUser.PasswordHash = hashedPassword;

        dbContext.Users.Add(newUser);
        await dbContext.SaveChangesAsync();

        await SendEmailConfirmationAsync(newUser);
    }
}