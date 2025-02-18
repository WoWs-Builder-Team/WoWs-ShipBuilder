namespace WoWsShipBuilder.Features.LinkShortening;

public class LinkShorteningOptions
{
    public const string SectionName = "LinkShortening";

    public string ApiKey { get; set; } = string.Empty;

    public string ApiUrl { get; set; } = string.Empty;

    public string UriPrefix { get; set; } = string.Empty;

    public string LinkBaseUrl { get; set; } = string.Empty;

    public int RequestTimeout { get; set; } = 3000;

    public int RateLimit { get; set; } = 5;
}
