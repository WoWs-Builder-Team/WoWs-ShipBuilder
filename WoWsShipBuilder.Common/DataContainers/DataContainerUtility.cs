using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.DataContainers;

public static class DataContainerUtility
{
    public static ShipDataContainer GetShipDataContainerFromBuild(Ship ship, IEnumerable<string> selectedModules, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
    {
        return ShipDataContainer.CreateFromShip(ship, Helpers.GetShipConfigurationFromBuild(selectedModules, shipConfiguration), modifiers);
    }

    public static ShipDataContainer GetStockShipDataContainer(Ship ship)
    {
        return ShipDataContainer.CreateFromShip(ship, Helpers.GetStockShipConfiguration(ship), new());
    }
}
