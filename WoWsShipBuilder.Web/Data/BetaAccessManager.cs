using WoWsShipBuilder.Core.Localization;

namespace WoWsShipBuilder.Web.Data;

public class BetaAccessManager : IBetaAccessManager
{
    public static readonly BetaAccessEntry ShipComparison = new("ship-comp", nameof(Translation.BETA_ShipComparison));
    public static readonly BetaAccessEntry AccelerationCharts = new("accel-charts", nameof(Translation.BETA_AccelerationCharts));

    public IEnumerable<BetaAccessEntry> ActiveBetas { get; } = new List<BetaAccessEntry>
    {
        ShipComparison,
        AccelerationCharts,
    };

    public BetaAccessEntry? FindBetaByCode(string code) => ActiveBetas.FirstOrDefault(b => b.Code == code);

    public bool IsBetaActive(BetaAccessEntry entry) => ActiveBetas.Contains(entry);
}

public sealed record BetaAccessEntry(string Code, string LocalizationKey);
