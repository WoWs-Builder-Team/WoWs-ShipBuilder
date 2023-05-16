using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Features.ShipStats;

public record ShipViewModelParams(Ship Ship, ShipSummary ShipSummary, Build? Build = null);
