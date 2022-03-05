using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.Data
{
    public record MainViewModelParams(Ship Ship, ShipSummary ShipSummary, Build? Build = null);
}
