namespace WoWsShipBuilder.Web.Data;

public class AdminOptions
{
    public const string SectionName = "AdminSettings";

    public string WgApiKey { get; set; } = string.Empty;

    public string[] AdminUsers { get; set; } = Array.Empty<string>();

    public string[] BuildCurators { get; set; } = Array.Empty<string>();
}
