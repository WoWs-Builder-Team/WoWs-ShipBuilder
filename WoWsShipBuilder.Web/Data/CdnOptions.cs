using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Web.Data;

public class CdnOptions
{
    public const string SectionName = "CDNSettings";

    public string Host { get; set; } = string.Empty;

    public ServerType Server { get; set; }
}
