using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

public record SecondaryBatteryUiDataContainer(List<SecondaryBatteryDataContainer>? Secondaries)
{
    public static SecondaryBatteryUiDataContainer FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
    {
        return new(SecondaryBatteryDataContainer.FromShip(ship, shipConfiguration, modifiers));
    }
}
