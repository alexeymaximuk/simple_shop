using MailKit.Security;
using MimeKit;
using Shop.Shared.Constants;
using MailKit.Net.Smtp;
using Shop.Users.Application.Interfaces;
using Shop.Users.Domain.Settings;
using Shop.Users.Infrastructure.Email.Constants;

namespace Shop.Users.Infrastructure.Email;

public partial class EmailSendingService(EmailSettings emailSettings, AppSettings appSettings) : IEmailSendingService
{
    public async Task SendPasswordResetMailAsync(string userEmail, string token)
    {
        var link = $"{appSettings.BaseUrl}/{AuthConstants.ResetPasswordRoute}?token={token}";

        var message = CreateDefaultMessageBody(userEmail);
        message.Subject = EmailTemplates.ResetPasswordSubject;
        message.Body = new TextPart("html") {Text = EmailTemplates.ResetPassword(link)};

        await SendEmail(message);
    }

    public async Task SendEmailConfirmationMailAsync(string userEmail, string token)
    {
        var link = $"{appSettings.BaseUrl}/{AuthConstants.ConfirmEmailRoute}?token={token}";

        var message = CreateDefaultMessageBody(userEmail);
        message.Subject = EmailTemplates.ConfirmEmailSubject;
        message.Body = new TextPart("html") {Text = EmailTemplates.ConfirmEmail(link)};

        await SendEmail(message);
    }

    public async Task SendEmailChangeMailAsync(string newEmail, string token)
    {
        var link = $"{appSettings.BaseUrl}/{AuthConstants.ConfirmEmailChangeRoute}?token={token}";

        var message = CreateDefaultMessageBody(newEmail);
        message.Subject = EmailTemplates.ChangeEmailSubject;
        message.Body = new TextPart("html") {Text = EmailTemplates.ChangeEmail(link)};
        
        await SendEmail(message);
    }

    public async Task SendYourPasswordWasRecentlyChanged(string email)
    {
        var message = CreateDefaultMessageBody(email);
        message.Subject = EmailTemplates.PasswordWasRecentlyChangedSubject;
        message.Body = new TextPart("html") {Text = EmailTemplates.PasswordWasRecentlyChanged()};

        await SendEmail(message);
    }
    
    private MimeMessage CreateDefaultMessageBody(string userEmail)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Shop", appSettings.EmailFrom));
        message.To.Add(new MailboxAddress("", userEmail));

        return message;
    }

    private async Task SendEmail(MimeMessage message)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(emailSettings.Host, emailSettings.Port, SecureSocketOptions.None);
        //await client.AuthenticateAsync(emailSettings.Username, emailSettings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}