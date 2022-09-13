using WoWsShipBuilder.Core.Builds;

namespace WoWsShipBuilder.Web.LinkShortening;

public interface ILinkShortener
{
    Task<ShorteningResult> CreateLinkForBuild(Build build);
}
