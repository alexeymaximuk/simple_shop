namespace Shop.Users.Settings;

public class AppSettings
{
    public const string SectionName = "App";

    public string BaseUrl { get; set; }
    public string EmailFrom { get; set; }
}