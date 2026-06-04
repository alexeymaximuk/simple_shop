namespace Shop.Users.Domain.Settings;

public class EmailSettings
{
    public const string SectionName = "Email";
    public required string Host { get; set; }
    public int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}