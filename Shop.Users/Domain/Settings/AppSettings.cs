namespace Shop.Users.Domain.Settings;

public class AppSettings
{
    public const string SectionName = "App";

    public required string BaseUrl { get; set; }
    public required string EmailFrom { get; set; }
}