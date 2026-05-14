namespace Shop.Users.Application.Interfaces;

public interface IEmailSendingService
{
    Task SendPasswordResetMailAsync(string userEmail, string token);
    Task SendEmailConfirmationMailAsync(string userEmail, string token);
    Task SendEmailChangeMailAsync(string newEmail, string token);
    Task SendYourPasswordWasRecentlyChanged(string userEmail);
}