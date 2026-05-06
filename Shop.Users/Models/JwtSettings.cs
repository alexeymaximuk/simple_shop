namespace Shop.Users.Models;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int TokenDuration { get; set; }
}