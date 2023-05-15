using WoWsShipBuilder.Common.Builds;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Common.ShipStats;

public record ShipViewModelParams(Ship Ship, ShipSummary ShipSummary, Build? Build = null);
