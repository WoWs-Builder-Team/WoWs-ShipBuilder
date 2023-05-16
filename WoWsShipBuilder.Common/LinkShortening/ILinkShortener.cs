using WoWsShipBuilder.Common.Builds;

namespace WoWsShipBuilder.Common.LinkShortening;

public interface ILinkShortener
{
    public bool IsAvailable { get; }

    Task<ShorteningResult> CreateLinkForBuild(Build build);

    Task<ShorteningResult> CreateShortLink(string link);
}
