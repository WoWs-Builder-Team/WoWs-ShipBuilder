using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

/// <summary>
/// The "Aimed Fire" anti-air mechanic. Charge builds up while enemy aircraft are inside any AA aura. Once the charge
/// is full, it is spent again to boost continuous and flak damage and to periodically destroy a share of the attacking
/// squadron outright.
/// </summary>
/// <remarks>
/// The raw charge numbers are meaningless on their own, so every charge quantity is exposed as the time it takes at
/// the matching rate instead. That keeps the displayed values correct even if the required charge ever stops being 100.
/// </remarks>
[DataContainer]
public partial class AimedFireDataContainer : DataContainerBase
{
    /// <summary>
    /// Gets or sets the time spent with aircraft in range before the mechanic activates.
    /// </summary>
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ChargeUpTime { get; set; }

    /// <summary>
    /// Gets or sets how long a full charge lasts once the mechanic is active.
    /// </summary>
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ActiveDuration { get; set; }

    /// <summary>
    /// Gets or sets the grace period without aircraft in range before the accumulated charge starts to decay.
    /// </summary>
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ChargeDecayDelay { get; set; }

    /// <summary>
    /// Gets or sets how long a full charge takes to decay away once the decay delay has elapsed.
    /// </summary>
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ChargeDecayTime { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal AuraDamageMultiplier { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal FlakDamageMultiplier { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal InstantDamageCooldown { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "InstantDamage", UnitKey = "PerCent", LocalizationKeyOverride = "Destroyer")]
    public decimal InstantDamageVsDestroyerPlanes { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "InstantDamage", UnitKey = "PerCent", LocalizationKeyOverride = "Cruiser")]
    public decimal InstantDamageVsCruiserPlanes { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "InstantDamage", UnitKey = "PerCent", LocalizationKeyOverride = "Battleship")]
    public decimal InstantDamageVsBattleshipPlanes { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "InstantDamage", UnitKey = "PerCent", LocalizationKeyOverride = "AirCarrier")]
    public decimal InstantDamageVsAirCarrierPlanes { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "InstantDamage", UnitKey = "PerCent", LocalizationKeyOverride = "Submarine")]
    public decimal InstantDamageVsSubmarinePlanes { get; set; }

    public static AimedFireDataContainer? FromAimedFire(AntiAirAimedFire? aimedFire)
    {
        if (aimedFire is null)
        {
            return null;
        }

        var aimedFireData = new AimedFireDataContainer
        {
            ChargeUpTime = ChargeDuration(aimedFire.RequiredCharge, aimedFire.ChargeGainRate),
            ActiveDuration = ChargeDuration(aimedFire.RequiredCharge, aimedFire.ChargeSpendingRate),
            ChargeDecayDelay = Math.Round(aimedFire.DecrementDelay, 1),
            ChargeDecayTime = ChargeDuration(aimedFire.RequiredCharge, aimedFire.DecrementRate),
            AuraDamageMultiplier = Math.Round(aimedFire.AuraDamageMultiplier, 2),
            FlakDamageMultiplier = Math.Round(aimedFire.BubbleDamageMultiplier, 2),
            InstantDamageCooldown = Math.Round(aimedFire.InstantDamageCooldown, 1),
            InstantDamageVsDestroyerPlanes = InstantDamage(aimedFire, ShipClass.Destroyer),
            InstantDamageVsCruiserPlanes = InstantDamage(aimedFire, ShipClass.Cruiser),
            InstantDamageVsBattleshipPlanes = InstantDamage(aimedFire, ShipClass.Battleship),
            InstantDamageVsAirCarrierPlanes = InstantDamage(aimedFire, ShipClass.AirCarrier),
            InstantDamageVsSubmarinePlanes = InstantDamage(aimedFire, ShipClass.Submarine),
        };

        aimedFireData.UpdateDataElements();
        return aimedFireData;
    }

    /// <summary>
    /// Converts a charge amount into the time it takes to accumulate or lose it at <paramref name="ratePerSecond"/>.
    /// A missing or non-positive rate would make the mechanic never reach that state, so it reports zero, which hides
    /// the row instead of dividing by zero.
    /// </summary>
    private static decimal ChargeDuration(decimal charge, decimal ratePerSecond) => ratePerSecond > 0 ? Math.Round(charge / ratePerSecond, 1) : 0;

    /// <summary>
    /// Reads the instant damage share for planes belonging to <paramref name="targetClass"/>. The dictionary is keyed
    /// by the owning ship class of the target squadron, not by the class of the ship shooting at it.
    /// </summary>
    private static decimal InstantDamage(AntiAirAimedFire aimedFire, ShipClass targetClass) => aimedFire.InstantDamagePercentage.TryGetValue(targetClass, out decimal percentage) ? Math.Round(percentage * 100, 2) : 0;
}
