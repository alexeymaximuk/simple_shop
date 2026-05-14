using Shop.Users.Application.DTOs.Auth;
using Shop.Users.Domain.Models;

namespace Shop.Users.Application.Interfaces;

public interface IEmailVerificationService
{
    Task ConfirmEmailAsync(string code);
    Task ChangeEmailRequestAsync(Guid id, ChangeEmailRequestDto dto);
    Task ChangeEmailConfirmAsync(string token);
    Task ResendConfirmationAsync(string email);
    Task SendEmailConfirmationAsync(User user);
}