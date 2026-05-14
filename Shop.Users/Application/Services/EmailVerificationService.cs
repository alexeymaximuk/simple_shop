using Shop.Shared.Constants;
using Shop.Shared.Exceptions;
using Shop.Shared.Helpers;
using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Application.Interfaces;
using Shop.Users.Domain.Models;
using Shop.Users.Infrastructure.Email;

namespace Shop.Users.Application.Services;

public class EmailVerificationService(
    IUserRepository userRepository,
    IEmailSendingService emailSendingService
    ) : IEmailVerificationService
{
    public async Task ChangeEmailRequestAsync(Guid id, ChangeEmailRequestDto dto)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null) throw new NotFoundException("User not found");

        user.PendingEmail = dto.NewEmail;

        user.EmailChangeToken = TokenGenerator.GenerateToken();
        user.EmailChangeTokenExpiry = DateTime.UtcNow.AddHours(AuthConstants.ChangeEmailConfirmationTokenExpiryHours);
        await userRepository.SaveChangesAsync();
        await emailSendingService.SendEmailChangeMailAsync(user.PendingEmail!, user.EmailChangeToken);
    }

    public async Task ChangeEmailConfirmAsync(string token)
    {
        var user = await userRepository.GetByEmailChangeTokenAsync(token);
        if (user == null) throw new NotFoundException("User with that email change token not found");
        if (user.PendingEmail == null) throw new InvalidRequestException("User has not been pending email");
        if (user.EmailChangeTokenExpiry == null || user.EmailChangeTokenExpiry < DateTime.UtcNow) throw new TokenExpiredException("Token expired");

        user.Email = user.PendingEmail;
        user.EmailChangeToken = null;
        user.EmailChangeTokenExpiry = null;
        user.IsEmailConfirmed = true;
        user.PendingEmail = null;

        await userRepository.SaveChangesAsync();
    }

    public async Task ConfirmEmailAsync(string emailConfirmationToken)
    {
        var user = await userRepository.GetByEmailConfirmationTokenAsync(emailConfirmationToken);
        if (user == null) throw new NotFoundException("Invalid token");
        if (user.EmailConfirmationTokenExpiry == null || user.EmailConfirmationTokenExpiry < DateTime.UtcNow) throw new TokenExpiredException("Token expired");

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;

        await userRepository.SaveChangesAsync();
    }

    public async Task ResendConfirmationAsync(string email)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user is null) throw new NotFoundException("User not found");
        if (user.IsEmailConfirmed) throw new InvalidRequestException("Email already confirmed");

        await SendEmailConfirmationAsync(user);
    }

    public async Task SendEmailConfirmationAsync(User user)
    {
        var expiry = DateTime.UtcNow.AddHours(AuthConstants.EmailConfirmationTokenExpiryHours);

        user.EmailConfirmationToken = TokenGenerator.GenerateToken();
        user.EmailConfirmationTokenExpiry = expiry;

        await userRepository.SaveChangesAsync();
        await emailSendingService.SendEmailConfirmationMailAsync(user.Email, user.EmailConfirmationToken);
    }
}