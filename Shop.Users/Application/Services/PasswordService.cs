using Microsoft.AspNetCore.Identity;
using Shop.Shared.Constants;
using Shop.Shared.Exceptions;
using Shop.Shared.Helpers;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;
using Shop.Users.Domain.Models;

namespace Shop.Users.Application.Services;

public class PasswordService(
    IUserRepository userRepository,
    IEmailSendingService emailSendingService,
    IPasswordHasher<User> passwordHasher
    ) : IPasswordService
{
    public async Task ChangePasswordRequestAsync(ResetPasswordRequestDto dto)
    {
        var user = await userRepository.GetByEmailAsync(dto.Email);
        if (user == null) throw new NotFoundException("User with that email not found");
        if (user.IsActive == false) throw new AccountDeactivatedException("User with that email not found");

        user.PasswordResetToken = TokenGenerator.GenerateToken();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(AuthConstants.ResetPasswordTokenExpiryHours);
        await userRepository.SaveChangesAsync();

        await emailSendingService.SendPasswordResetMailAsync(dto.Email, user.PasswordResetToken);
    }

    public async Task ValidateChangePasswordRequestAsync(string token)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(token);
        if (user == null) throw new NotFoundException("User with that password reset token not found");
        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow) throw new TokenExpiredException("Token expired");
    }

    public async Task ChangePasswordAsync(ResetPasswordDto dto)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(dto.Token);
        if (user == null) throw new NotFoundException("User with that password reset token not found");
        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow) throw new TokenExpiredException("Token expired");

        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);
        await userRepository.SaveChangesAsync();

        await emailSendingService.SendYourPasswordWasRecentlyChanged(user.Email);
    }
}