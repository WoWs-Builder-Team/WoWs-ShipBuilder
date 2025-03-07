using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Features.RecommendedBuilds;

public record RecommendedBuildViewModel(ShipBuildViewModel ViewModel)
{
    public string Notes { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public bool IsNew { get; set; }
}
