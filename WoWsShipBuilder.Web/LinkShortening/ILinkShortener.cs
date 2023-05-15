using WoWsShipBuilder.Common.Builds;

namespace WoWsShipBuilder.Web.LinkShortening;

public interface ILinkShortener
{
    Task<ShorteningResult> CreateLinkForBuild(Build build);

    Task<ShorteningResult> CreateShortLink(string link);

    public bool IsAvailable { get; }
}
