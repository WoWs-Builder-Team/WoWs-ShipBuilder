namespace WoWsShipBuilder.Web.Infrastructure;

public class AdminOptions
{
    public const string SectionName = "AdminSettings";

    public string WgApiKey { get; set; } = string.Empty;

    public string[] AllowedUsers { get; set; } = Array.Empty<string>();
}
