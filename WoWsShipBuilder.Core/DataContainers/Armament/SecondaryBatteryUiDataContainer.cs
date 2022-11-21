using System.Collections.Generic;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Core.DataContainers;

public record SecondaryBatteryUiDataContainer(List<SecondaryBatteryDataContainer>? Secondaries)
{
    private string expanderKey = default!;

    public bool IsExpanderOpen
    {
        get => ShipDataContainer.ExpanderStateMapper[expanderKey];
        set => ShipDataContainer.ExpanderStateMapper[expanderKey] = value;
    }

    public static SecondaryBatteryUiDataContainer FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
    {
        var uiContainer = new SecondaryBatteryUiDataContainer(SecondaryBatteryDataContainer.FromShip(ship, shipConfiguration, modifiers))
        {
            expanderKey = $"{ship.Index}_SEC",
        };
        if (!ShipDataContainer.ExpanderStateMapper.ContainsKey(uiContainer.expanderKey))
        {
            ShipDataContainer.ExpanderStateMapper[uiContainer.expanderKey] = true;
        }

        return uiContainer;
    }
}
