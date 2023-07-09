using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Features.RecommendedBuilds;

public record RecommendedBuild(Guid Id, Build Build, string Notes);
