using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Web.Infrastructure.BetaAccess;

public class BetaAccessManager : IBetaAccessManager
{
    public static readonly BetaAccessEntry ShipComparison = new("ship-comp", nameof(Translation.BETA_ShipComparison));

    public IEnumerable<BetaAccessEntry> ActiveBetas { get; } = new List<BetaAccessEntry>
    {
        ShipComparison,
    };

    public BetaAccessEntry? FindBetaByCode(string code) => ActiveBetas.FirstOrDefault(b => b.Code == code);

    public bool IsBetaActive(BetaAccessEntry entry) => ActiveBetas.Contains(entry);
}

public sealed record BetaAccessEntry(string Code, string LocalizationKey);
