using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Web.Features.BetaAccess;

public class BetaAccessManager : IBetaAccessManager
{
    public static readonly BetaAccessEntry UserAuth = new("user-auth", nameof(Translation.BETA_UserAuth));

    public IEnumerable<BetaAccessEntry> ActiveBetas { get; } = new List<BetaAccessEntry>
    {
        UserAuth,
    };

    public BetaAccessEntry? FindBetaByCode(string code) => this.ActiveBetas.FirstOrDefault(b => b.Code == code);

    public bool IsBetaActive(BetaAccessEntry entry) => this.ActiveBetas.Contains(entry);
}

public sealed record BetaAccessEntry(string Code, string LocalizationKey);
