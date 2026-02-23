namespace WoWsShipBuilder.Desktop.Infrastructure.StaticConfiguration;

internal sealed class ApplicationOptions
{
    public string SentryDsn { get; set; } = string.Empty;

    public string? WgApiKey { get; set; }
}
