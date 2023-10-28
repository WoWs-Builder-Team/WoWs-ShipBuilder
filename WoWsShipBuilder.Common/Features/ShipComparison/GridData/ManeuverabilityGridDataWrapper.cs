using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class ManeuverabilityGridDataWrapper
{
    public ManeuverabilityGridDataWrapper(ManeuverabilityDataContainer maneuverability)
    {
        this.MaxSpeed = maneuverability.ManeuverabilityMaxSpeed != 0 ? maneuverability.ManeuverabilityMaxSpeed : maneuverability.ManeuverabilitySubsMaxSpeedOnSurface;
        this.MaxSpeedAtPeriscopeDepth = maneuverability.ManeuverabilitySubsMaxSpeedAtPeriscope;
        this.MaxSpeedAtMaxDepth = maneuverability.ManeuverabilitySubsMaxSpeedAtMaxDepth;
        this.MaxReverseSpeed = maneuverability.ManeuverabilityMaxReverseSpeed != 0 ? maneuverability.ManeuverabilityMaxReverseSpeed : maneuverability.ManeuverabilitySubsMaxReverseSpeedOnSurface;
        this.MaxReverseSpeedAtPeriscopeDepth = maneuverability.ManeuverabilitySubsMaxReverseSpeedAtPeriscope;
        this.MaxReverseSpeedAtMaxDepth = maneuverability.ManeuverabilitySubsMaxReverseSpeedAtMaxDepth;
        this.MaxDiveSpeed = maneuverability.ManeuverabilitySubsMaxDiveSpeed;
        this.DivingPlaneShiftTime = maneuverability.ManeuverabilitySubsDivingPlaneShiftTime;
        this.RudderShiftTime = maneuverability.ManeuverabilityRudderShiftTime;
        this.TurningCircle = maneuverability.ManeuverabilityTurningCircle;
        this.TimeToFullAhead = maneuverability.ForwardMaxSpeedTime;
        this.TimeToFullReverse = maneuverability.ReverseMaxSpeedTime;
        this.RudderProtection = maneuverability.RudderBlastProtection;
        this.EngineProtection = maneuverability.EngineBlastProtection;
    }

    public decimal MaxSpeed { get; }

    public decimal MaxSpeedAtPeriscopeDepth { get; }

    public decimal MaxSpeedAtMaxDepth { get; }

    public decimal MaxReverseSpeed { get; }

    public decimal MaxReverseSpeedAtPeriscopeDepth { get; }

    public decimal MaxReverseSpeedAtMaxDepth { get; }

    public decimal MaxDiveSpeed { get; }

    public decimal DivingPlaneShiftTime { get; }

    public decimal RudderShiftTime { get; }

    public decimal TurningCircle { get; }

    public decimal TimeToFullAhead { get; }

    public decimal TimeToFullReverse { get; }

    public decimal RudderProtection { get; }

    public decimal EngineProtection { get; }
}
