using WoWsShipBuilder.Common.Features.Builds;

namespace WoWsShipBuilder.Common.Features.LinkShortening;

public interface ILinkShortener
{
    public bool IsAvailable { get; }

    Task<ShorteningResult> CreateLinkForBuild(Build build);

    Task<ShorteningResult> CreateShortLink(string link);
}
