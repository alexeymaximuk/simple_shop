namespace Shop.Shared.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int TokenDuration { get; set; }
}