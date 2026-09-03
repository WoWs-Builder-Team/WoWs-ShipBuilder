using System.Collections.Immutable;
using FluentAssertions;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Test.Features.DataContainers.AimedFireDataContainerTests;

/// <summary>
/// Covers the "Aimed Fire" anti-air mechanic added by DataStructures 6.4.0.
/// </summary>
[TestFixture]
public class FromAimedFire
{
    private static readonly ImmutableDictionary<ShipClass, decimal> CanonicalInstantDamage = ImmutableDictionary<ShipClass, decimal>.Empty
        .Add(ShipClass.Destroyer, 0.05m)
        .Add(ShipClass.Cruiser, 0.035m)
        .Add(ShipClass.Battleship, 0.035m)
        .Add(ShipClass.AirCarrier, 0.035m)
        .Add(ShipClass.Submarine, 0.035m);

    [Test]
    public void NoAimedFire_ProducesNoContainer()
    {
        AimedFireDataContainer.FromAimedFire(null).Should().BeNull();
    }

    /// <summary>
    /// The raw charge numbers are meaningless on their own, so they are shown as the time they take at the matching
    /// rate. With the values the game ships, that is 40 s to charge, 40 s of uptime and 20 s to decay again.
    /// </summary>
    [Test]
    public void CanonicalValues_AreReportedAsTimes()
    {
        var container = AimedFireDataContainer.FromAimedFire(AimedFire())!;

        container.ChargeUpTime.Should().Be(40m);
        container.ActiveDuration.Should().Be(40m);
        container.ChargeDecayTime.Should().Be(20m);
    }

    [Test]
    public void CanonicalValues_PassTheRemainingStatsThrough()
    {
        var container = AimedFireDataContainer.FromAimedFire(AimedFire())!;

        container.ChargeDecayDelay.Should().Be(130m);
        container.InstantDamageCooldown.Should().Be(5m);
        container.AuraDamageMultiplier.Should().Be(1.5m);
        container.FlakDamageMultiplier.Should().Be(2m);
    }

    /// <summary>
    /// A charge quantity is only meaningful as a time if the matching rate is positive. A missing rate must not divide
    /// by zero, and reporting zero also makes DataContainerBase.ShouldAdd drop the row instead of showing a bogus
    /// duration.
    /// </summary>
    [Test]
    public void AZeroRate_DoesNotThrowAndHidesTheAffectedRows()
    {
        var container = AimedFireDataContainer.FromAimedFire(AimedFire(chargeGainRate: 0m, chargeSpendingRate: 0m, decrementRate: 0m))!;

        container.ChargeUpTime.Should().Be(0m);
        container.ActiveDuration.Should().Be(0m);
        container.ChargeDecayTime.Should().Be(0m);

        var keys = container.DataElements.OfType<KeyValueUnitDataElement>().Select(element => element.Key).ToList();
        keys.Should().NotContain("ShipStats_ChargeUpTime");
        keys.Should().NotContain("ShipStats_ActiveDuration");
        keys.Should().NotContain("ShipStats_ChargeDecayTime");
        keys.Should().Contain("ShipStats_ChargeDecayDelay", "the delay is not derived from a rate and stays visible");
    }

    /// <summary>
    /// The dictionary is keyed by the owning ship class of the target squadron, not by the class of the ship firing.
    /// Destroyer-owned planes take the higher share.
    /// </summary>
    [Test]
    public void InstantDamage_IsReportedPerTargetShipClass()
    {
        var container = AimedFireDataContainer.FromAimedFire(AimedFire())!;

        container.InstantDamageVsDestroyerPlanes.Should().Be(5m);
        container.InstantDamageVsCruiserPlanes.Should().Be(3.5m);
        container.InstantDamageVsBattleshipPlanes.Should().Be(3.5m);
        container.InstantDamageVsAirCarrierPlanes.Should().Be(3.5m);
        container.InstantDamageVsSubmarinePlanes.Should().Be(3.5m);
    }

    [Test]
    public void AClassWithoutAnInstantDamageEntry_IsLeftOutOfTheGroup()
    {
        var onlyDestroyers = ImmutableDictionary<ShipClass, decimal>.Empty.Add(ShipClass.Destroyer, 0.05m);

        var container = AimedFireDataContainer.FromAimedFire(AimedFire(instantDamagePercentage: onlyDestroyers))!;

        container.InstantDamageVsCruiserPlanes.Should().Be(0m);
        var group = container.DataElements.OfType<GroupedDataElement>().Single();
        group.Children.OfType<KeyValueUnitDataElement>().Select(child => child.Key).Should().Equal("ShipStats_Destroyer");
    }

    /// <summary>
    /// Regression guard for the rounding: a whole number of seconds must not pick up a trailing decimal place, because
    /// the value reaches the UI as a plain decimal ToString.
    /// </summary>
    [Test]
    public void AWholeNumberOfSeconds_IsNotPaddedWithADecimalPlace()
    {
        var container = AimedFireDataContainer.FromAimedFire(AimedFire())!;

        container.DataElements.OfType<KeyValueUnitDataElement>().First(element => element.Key.Equals("ShipStats_ChargeUpTime", StringComparison.Ordinal))
            .Value.Should().Be("40");
    }

    /// <summary>
    /// Pins the contract of the DataElement source generator: which keys it emits, in which order, with which units.
    /// Every key has to exist in Translation.resx, so a rename here is a silently missing label in the UI.
    /// </summary>
    [Test]
    public void DataElements_MatchTheGeneratedKeysAndUnits()
    {
        var container = AimedFireDataContainer.FromAimedFire(AimedFire())!;

        container.DataElements.Should().HaveCount(8);

        container.DataElements.OfType<KeyValueUnitDataElement>().Select(element => (element.Key, element.Unit)).Should().Equal(
            ("ShipStats_ChargeUpTime", "Unit_S"),
            ("ShipStats_ActiveDuration", "Unit_S"),
            ("ShipStats_ChargeDecayDelay", "Unit_S"),
            ("ShipStats_ChargeDecayTime", "Unit_S"),
            ("ShipStats_InstantDamageCooldown", "Unit_S"));

        container.DataElements.OfType<KeyValueDataElement>().Select(element => element.Key).Should().Equal(
            "ShipStats_AuraDamageMultiplier",
            "ShipStats_FlakDamageMultiplier");

        var group = container.DataElements.OfType<GroupedDataElement>().Single();
        group.Key.Should().Be("ShipStats_InstantDamage");
        group.Children.OfType<KeyValueUnitDataElement>().Select(child => (child.Key, child.Unit)).Should().Equal(
            ("ShipStats_Destroyer", "Unit_PerCent"),
            ("ShipStats_Cruiser", "Unit_PerCent"),
            ("ShipStats_Battleship", "Unit_PerCent"),
            ("ShipStats_AirCarrier", "Unit_PerCent"),
            ("ShipStats_Submarine", "Unit_PerCent"));
    }

    private static AntiAirAimedFire AimedFire(decimal chargeGainRate = 2.5m, decimal chargeSpendingRate = 2.5m, decimal decrementRate = 5m, ImmutableDictionary<ShipClass, decimal>? instantDamagePercentage = null) => new()
    {
        RequiredCharge = 100m,
        ChargeGainRate = chargeGainRate,
        ChargeSpendingRate = chargeSpendingRate,
        DecrementDelay = 130m,
        DecrementRate = decrementRate,
        InstantDamageCooldown = 5m,
        InstantDamagePercentage = instantDamagePercentage ?? CanonicalInstantDamage,
        AuraDamageMultiplier = 1.5m,
        BubbleDamageMultiplier = 2m,
    };
}
