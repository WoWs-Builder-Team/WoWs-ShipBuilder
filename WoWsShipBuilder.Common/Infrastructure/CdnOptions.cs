using WoWsShipBuilder.Infrastructure.GameData;

namespace WoWsShipBuilder.Infrastructure;

public class CdnOptions
{
    public const string SectionName = "CDNSettings";

    public string Host { get; set; } = string.Empty;

    public ServerType Server { get; set; }

    public bool UseLocalFiles { get; set; }

    public string ShipImagePath { get; set; } = string.Empty;
}
