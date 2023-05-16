using WoWsShipBuilder.Common.Features.Builds;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Common.Features.ShipStats;

public record ShipViewModelParams(Ship Ship, ShipSummary ShipSummary, Build? Build = null);
