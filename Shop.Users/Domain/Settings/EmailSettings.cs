namespace Shop.Users.Domain.Settings;

public class EmailSettings
{
    public const string SectionName = "Email";
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}