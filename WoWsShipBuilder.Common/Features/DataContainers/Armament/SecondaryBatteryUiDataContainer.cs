using System.Collections.Immutable;
using System.Globalization;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class SecondaryBatteryUiDataContainer : DataContainerBase
{
    private SecondaryBatteryUiDataContainer()
    {
    }

    [DataElementType(DataElementTypes.KeyValue, ValueTextKind = TextKind.AppLocalizationKey)]
    public string ShellType { get; init; } = string.Empty;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal Range { get; init; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Overall", UnitKey = "ShotsPerMinute", LocalizationKeyOverride = "RoF")]
    public decimal TotalRoF { get; init; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "Overall", LocalizationKeyOverride = "Dpm")]
    public string TotalDpm { get; init; } = string.Empty;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Overall", UnitKey = "FPM", LocalizationKeyOverride = "PotentialFpm")]
    public decimal TotalFpm { get; init; }

    public required ImmutableList<SecondaryBatteryDataContainer> Secondaries { get; init; }

    public static SecondaryBatteryUiDataContainer FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<Modifier> modifiers)
    {
        var secondaries = SecondaryBatteryDataContainer.FromShip(ship, shipConfiguration, modifiers);
        var secondaryBatteryUiDataContainer = new SecondaryBatteryUiDataContainer
        {
            Secondaries = secondaries?.ToImmutableList() ?? ImmutableList<SecondaryBatteryDataContainer>.Empty,
            ShellType = secondaries?[0].Shell?.Type ?? string.Empty,
            Range = secondaries?[0].Range ?? 0,
            TotalRoF = secondaries?.Sum(x => x.RoF) ?? 0,
            TotalDpm = secondaries?.Sum(x => x.Dpm).ToString("n0", SetNumberGroupSeparator()) ?? "",
            TotalFpm = secondaries?.Sum(x => x.PotentialFpm) ?? 0,
        };
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
