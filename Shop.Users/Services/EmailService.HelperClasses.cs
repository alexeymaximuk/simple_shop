using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Shop.Users.Services;

public partial class EmailService
{
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