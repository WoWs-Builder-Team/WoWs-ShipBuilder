using System.Globalization;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial record SecondaryBatteryUiDataContainer(List<SecondaryBatteryDataContainer>? Secondaries) : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValue, ValueTextKind = TextKind.AppLocalizationKey)]
    public string ShellType { get; } = Secondaries?[0].Shell?.Type ?? string.Empty;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal Range { get; } = Secondaries?[0].Range ?? 0;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Overall", UnitKey = "ShotsPerMinute", LocalizationKeyOverride = "RoF")]
    public decimal TotalRoF { get; } = Secondaries?.Sum(x => x.RoF) ?? 0;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Overall", LocalizationKeyOverride = "Dpm")]
    public string TotalDpm { get; } = Secondaries?.Sum(x => x.Dpm).ToString("n0", SetNumberGroupSeparator()) ?? "";

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Overall", UnitKey = "FPM", LocalizationKeyOverride = "PotentialFpm")]
    public decimal TotalFpm { get; } = Secondaries?.Sum(x => x.PotentialFpm) ?? 0;

    public static SecondaryBatteryUiDataContainer FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
    {
        var secondaryBatteryUiDataContainer = new SecondaryBatteryUiDataContainer(SecondaryBatteryDataContainer.FromShip(ship, shipConfiguration, modifiers));
        secondaryBatteryUiDataContainer.UpdateDataElements();
        return secondaryBatteryUiDataContainer;
    }

    private static NumberFormatInfo SetNumberGroupSeparator()
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = "'";
        return nfi;
    }
}
