using WoWsShipBuilder.Infrastructure.DataTransfer;

namespace WoWsShipBuilder.Features.BallisticCharts.Data;

public sealed record ChartsDataWrapper(ShipBuildContainer ShipBuildContainer, Dictionary<string, ChartCacheData> SelectedShells);
