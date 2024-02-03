using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

public sealed class ShipDataContainer(string index) : IEquatable<ShipDataContainer>
{
    public string Index { get; } = index;

    public SurvivabilityDataContainer SurvivabilityDataContainer { get; init; } = default!;

    public MainBatteryDataContainer? MainBatteryDataContainer { get; init; }

    public SecondaryBatteryUiDataContainer SecondaryBatteryUiDataContainer { get; init; } = default!;

    public PingerGunDataContainer? PingerGunDataContainer { get; init; }

    public TorpedoArmamentDataContainer? TorpedoArmamentDataContainer { get; init; }

    public AirstrikeDataContainer? AirstrikeDataContainer { get; init; }

    public AirstrikeDataContainer? AswAirstrikeDataContainer { get; init; }

    public DepthChargesLauncherDataContainer? DepthChargeLauncherDataContainer { get; init; }

    public ImmutableList<CvAircraftDataContainer>? CvAircraftDataContainer { get; init; }

    public ManeuverabilityDataContainer ManeuverabilityDataContainer { get; init; } = default!;

    public ConcealmentDataContainer ConcealmentDataContainer { get; init; } = default!;

    public AntiAirDataContainer? AntiAirDataContainer { get; init; }

    public ImmutableList<object> SecondColumnContent { get; private set; } = ImmutableList<object>.Empty;

    public SpecialAbilityDataContainer? SpecialAbilityDataContainer { get; init; }

    public static ShipDataContainer CreateFromShip(Ship ship, ImmutableList<ShipUpgrade> shipConfiguration, ImmutableList<Modifier> modifiers)
    {
        var shipDataContainer = new ShipDataContainer(ship.Index)
        {
            // Main weapons
            MainBatteryDataContainer = MainBatteryDataContainer.FromShip(ship, shipConfiguration, modifiers),
            TorpedoArmamentDataContainer = TorpedoArmamentDataContainer.FromShip(ship, shipConfiguration, modifiers),
            CvAircraftDataContainer = DataContainers.CvAircraftDataContainer.FromShip(ship, shipConfiguration, modifiers)?.ToImmutableList(),
            PingerGunDataContainer = PingerGunDataContainer.FromShip(ship, shipConfiguration, modifiers),

            // Secondary weapons
            SecondaryBatteryUiDataContainer = SecondaryBatteryUiDataContainer.FromShip(ship, shipConfiguration, modifiers),
            AntiAirDataContainer = AntiAirDataContainer.FromShip(ship, shipConfiguration, modifiers),
            AirstrikeDataContainer = AirstrikeDataContainer.FromShip(ship, modifiers, false),
            AswAirstrikeDataContainer = AirstrikeDataContainer.FromShip(ship, modifiers, true),
            DepthChargeLauncherDataContainer = DepthChargesLauncherDataContainer.FromShip(ship, shipConfiguration, modifiers),

            // Misc
            ManeuverabilityDataContainer = ManeuverabilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
            ConcealmentDataContainer = ConcealmentDataContainer.FromShip(ship, shipConfiguration, modifiers),
            SurvivabilityDataContainer = SurvivabilityDataContainer.FromShip(ship, shipConfiguration, modifiers),
            SpecialAbilityDataContainer = SpecialAbilityDataContainer.FromShip(ship, shipConfiguration),
        };

        var secondColumnContent = new List<object?>
            {
                shipDataContainer.AntiAirDataContainer,
                shipDataContainer.AirstrikeDataContainer,
                shipDataContainer.AswAirstrikeDataContainer,
                shipDataContainer.DepthChargeLauncherDataContainer,
            }.Where(item => item != null)
            .Cast<object>()
            .ToList();

        if (!shipDataContainer.SecondaryBatteryUiDataContainer.Secondaries.IsEmpty)
        {
            secondColumnContent.Insert(0, shipDataContainer.SecondaryBatteryUiDataContainer);
        }

        shipDataContainer.SecondColumnContent = secondColumnContent.ToImmutableList();

        return shipDataContainer;
    }

    public bool Equals(ShipDataContainer? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.Index == other.Index &&
               this.SurvivabilityDataContainer.Equals(other.SurvivabilityDataContainer) &&
               Equals(this.MainBatteryDataContainer, other.MainBatteryDataContainer) &&
               this.SecondaryBatteryUiDataContainer.Equals(other.SecondaryBatteryUiDataContainer) &&
               Equals(this.PingerGunDataContainer, other.PingerGunDataContainer) &&
               Equals(this.TorpedoArmamentDataContainer, other.TorpedoArmamentDataContainer) &&
               Equals(this.AirstrikeDataContainer, other.AirstrikeDataContainer) &&
               Equals(this.AswAirstrikeDataContainer, other.AswAirstrikeDataContainer) &&
               Equals(this.DepthChargeLauncherDataContainer, other.DepthChargeLauncherDataContainer) &&
               Equals(this.CvAircraftDataContainer, other.CvAircraftDataContainer) &&
               this.ManeuverabilityDataContainer.Equals(other.ManeuverabilityDataContainer) &&
               this.ConcealmentDataContainer.Equals(other.ConcealmentDataContainer) &&
               Equals(this.AntiAirDataContainer, other.AntiAirDataContainer) &&
               Equals(this.SpecialAbilityDataContainer, other.SpecialAbilityDataContainer);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is ShipDataContainer other && this.Equals(other));
    }

    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        hashCode.Add(this.Index);
        hashCode.Add(this.SurvivabilityDataContainer);
        hashCode.Add(this.MainBatteryDataContainer);
        hashCode.Add(this.SecondaryBatteryUiDataContainer);
        hashCode.Add(this.PingerGunDataContainer);
        hashCode.Add(this.TorpedoArmamentDataContainer);
        hashCode.Add(this.AirstrikeDataContainer);
        hashCode.Add(this.AswAirstrikeDataContainer);
        hashCode.Add(this.DepthChargeLauncherDataContainer);
        hashCode.Add(this.CvAircraftDataContainer);
        hashCode.Add(this.ManeuverabilityDataContainer);
        hashCode.Add(this.ConcealmentDataContainer);
        hashCode.Add(this.AntiAirDataContainer);
        hashCode.Add(this.SpecialAbilityDataContainer);
        return hashCode.ToHashCode();
    }
}
