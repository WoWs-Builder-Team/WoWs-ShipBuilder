using System.Globalization;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;

namespace WoWsShipBuilder.Features.DataContainers;

/// <summary>
/// Classic charge-based consumable: a fixed number of charges, each active for a work time.
/// </summary>
[DataContainer]
public partial class RegularConsumableDataContainer : ConsumableDataContainer
{
    [DataElementType(DataElementTypes.KeyValue)]
    public string NumberOfUses { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal PreparationTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal Cooldown { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal WorkTime { get; set; }

    internal static RegularConsumableDataContainer Create(ConsumableState state, int slot, int numConsumables)
    {
        var container = new RegularConsumableDataContainer
        {
            Name = state.LocalizationKey,
            IconName = state.IconName,
            Slot = slot,
            NumberOfUses = numConsumables != -1 ? state.Uses.ToString(CultureInfo.InvariantCulture) : "∞",
            Cooldown = Math.Round(state.Cooldown, 1),
            PreparationTime = Math.Round(state.PrepTime, 1),
            WorkTime = Math.Round(state.WorkTime, 1),
            Modifiers = state.ConsumableModifiers,
        };

        container.UpdateDataElements();
        return container;
    }
}
