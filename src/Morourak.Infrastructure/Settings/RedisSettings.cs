using System.ComponentModel.DataAnnotations;

namespace Morourak.Infrastructure.Settings;

public class RedisSettings
{
    [Required(ErrorMessage = "Redis connection string is required.")]
    public string ConnectionString { get; set; } = string.Empty;
    public double DefaultExpirationMinutes { get; set; } = 10;
}
