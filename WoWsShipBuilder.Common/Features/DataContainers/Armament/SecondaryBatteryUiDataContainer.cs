using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial record SecondaryBatteryUiDataContainer(List<SecondaryBatteryDataContainer>? Secondaries) : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValue)]
    public string ShellType { get; } = Secondaries?[0].Shell?.Type ?? string.Empty;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
    public decimal Range { get; } = Secondaries?[0].Range ?? 0;

    public static SecondaryBatteryUiDataContainer FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
    {
        var secondaryBatteryUiDataContainer = new SecondaryBatteryUiDataContainer(SecondaryBatteryDataContainer.FromShip(ship, shipConfiguration, modifiers));
        secondaryBatteryUiDataContainer.UpdateDataElements();
        return secondaryBatteryUiDataContainer;
    }
}
