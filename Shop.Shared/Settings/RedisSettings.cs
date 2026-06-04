namespace Shop.Shared.Settings;

public class RedisSettings
{
    public const string SectionName = "Redis";
    public required string ConnectionUrl { get; set; }
    public int SessionDurationMinutes { get; set; } = 60;
}