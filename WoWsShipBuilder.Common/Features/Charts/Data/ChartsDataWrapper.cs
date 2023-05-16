using WoWsShipBuilder.Infrastructure.DataTransfer;

namespace WoWsShipBuilder.Features.Charts.Data;

public sealed record ChartsDataWrapper(ShipBuildContainer ShipBuildContainer, Dictionary<string, ChartCacheData> SelectedShells);
