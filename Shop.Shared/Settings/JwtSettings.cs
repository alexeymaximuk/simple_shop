namespace Shop.Shared.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int TokenDuration { get; set; }
}