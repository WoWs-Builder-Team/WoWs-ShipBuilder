using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Features.LinkShortening;

public interface ILinkShortener
{
    Task<LinkShorteningResult> CreateLinkForBuild(Build build);

    Task<LinkShorteningResult> CreateShortLink(string link);
}
