using WoWsShipBuilder.Common.Infrastructure.DataTransfer;

namespace WoWsShipBuilder.Common.Features.Charts.Data;

public sealed record ChartsDataWrapper(ShipBuildContainer ShipBuildContainer, Dictionary<string, ChartCacheData> SelectedShells);
