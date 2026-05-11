using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Shop.Shared.Constants;
using Shop.Users.Constants;
using Shop.Users.Services.Interfaces;
using Shop.Users.Settings;

namespace Shop.Users.Services;

public partial class EmailService(EmailSettings emailSettings, AppSettings appSettings) : IEmailService
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
}