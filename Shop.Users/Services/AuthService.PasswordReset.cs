using Microsoft.EntityFrameworkCore;
using Shop.Shared.Constants;
using Shop.Shared.Exceptions;
using Shop.Users.DTOs.Auth;

namespace Shop.Users.Services;

public partial class AuthService
{
    public async Task ChangePasswordRequestAsync(ResetPasswordRequestDto dto)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (user == null) throw new NotFoundException("User with that id not found");

        var passwordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetToken = passwordResetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(AuthConstants.ResetPasswordTokenExpiryHours);
        await dbContext.SaveChangesAsync();

        await emailService.SendPasswordResetMailAsync(dto.Email, passwordResetToken);
    }
    
    public async Task ValidateChangePasswordRequestAsync(string token)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.PasswordResetToken == token);
        if (user == null) throw new NotFoundException("User with that password reset token not found");
        if (user.PasswordResetTokenExpiry < DateTime.UtcNow) throw new TokenExpiredException("Token expired");
    }

    public async Task ChangePasswordAsync(ResetPasswordDto dto)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.PasswordResetToken == dto.Token);
        if (user == null) throw new NotFoundException("User with that password reset token not found");
        if (user.PasswordResetTokenExpiry < DateTime.UtcNow) throw new TokenExpiredException("Token expired");

        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        var newPasswordHash = passwordHasher.HashPassword(user, dto.NewPassword);
        user.PasswordHash = newPasswordHash;
        await dbContext.SaveChangesAsync();

        await emailService.SendYourPasswordWasRecentlyChanged(user.Email);
    }
}