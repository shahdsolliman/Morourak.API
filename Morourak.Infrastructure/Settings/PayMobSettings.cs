namespace Morourak.Infrastructure.Settings;

public class PayMobSettings
{
    public string ApiKey { get; set; } = default!;
    public string IframeId { get; set; } = default!;
    public string IntegrationId { get; set; } = default!;
    public string HmacSecret { get; set; } = default!;
    public string BaseUrl { get; set; } = "https://accept.paymob.com/api/";
}
