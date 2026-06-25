using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;

namespace WoWsShipBuilder.Features.DataContainers;

/// <summary>
/// Time-based ("capacity") consumable: no discrete charges, instead a total usage time pool that is
/// spent while active and regenerates over time.
/// </summary>
[DataContainer]
public partial class TimeBasedConsumableDataContainer : ConsumableDataContainer
{
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal PreparationTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal Cooldown { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal TimeBasedActiveTime { get; set; }

    internal static TimeBasedConsumableDataContainer Create(ConsumableState state, int slot)
    {
        var container = new TimeBasedConsumableDataContainer
        {
            Name = state.LocalizationKey,
            IconName = state.IconName,
            Slot = slot,
            Cooldown = Math.Round(state.Cooldown, 1),
            PreparationTime = Math.Round(state.PrepTime, 1),
            TimeBasedActiveTime = Math.Round(state.TimeBasedActiveTime, 1),
            Modifiers = state.ConsumableModifiers,
        };

        container.UpdateDataElements();
        return container;
    }
}
