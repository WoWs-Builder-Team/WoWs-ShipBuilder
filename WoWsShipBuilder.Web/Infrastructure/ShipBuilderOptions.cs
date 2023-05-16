namespace WoWsShipBuilder.Web.Infrastructure;

public class ShipBuilderOptions
{
    public const string SectionName = "ShipBuilder";

    public string LiveServerUrl { get; set; } = string.Empty;

    public string PtsServerUrl { get; set; } = string.Empty;

    public string MinimapRendererUrl { get; set; } = string.Empty;
}
