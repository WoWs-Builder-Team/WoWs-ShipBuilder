using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Core.Data;

public record ShipViewModelParams(Ship Ship, ShipSummary ShipSummary, Build? Build = null);
