namespace WoWsShipBuilder.Web.Infrastructure.BetaAccess;

public interface IBetaAccessManager
{
    public IEnumerable<BetaAccessEntry> ActiveBetas { get; }

    BetaAccessEntry? FindBetaByCode(string code);

    bool IsBetaActive(BetaAccessEntry entry);
}
