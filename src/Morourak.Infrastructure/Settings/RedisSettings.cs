namespace Morourak.Infrastructure.Settings;

public class RedisSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public double DefaultExpirationMinutes { get; set; } = 10;
}
