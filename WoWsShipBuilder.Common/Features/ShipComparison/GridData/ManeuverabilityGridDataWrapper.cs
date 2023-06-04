using WoWsShipBuilder.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class ManeuverabilityGridDataWrapper
{
    public ManeuverabilityGridDataWrapper(ManeuverabilityDataContainer maneuverability)
    {
        MaxSpeed = maneuverability.ManeuverabilityMaxSpeed != 0 ? maneuverability.ManeuverabilityMaxSpeed : maneuverability.ManeuverabilitySubsMaxSpeedOnSurface;
        MaxSpeedAtPeriscopeDepth = maneuverability.ManeuverabilitySubsMaxSpeedAtPeriscope;
        MaxSpeedAtMaxDepth = maneuverability.ManeuverabilitySubsMaxSpeedAtMaxDepth;
        MaxReverseSpeed = maneuverability.ManeuverabilityMaxReverseSpeed != 0 ? maneuverability.ManeuverabilityMaxReverseSpeed : maneuverability.ManeuverabilitySubsMaxReverseSpeedOnSurface;
        MaxReverseSpeedAtPeriscopeDepth = maneuverability.ManeuverabilitySubsMaxReverseSpeedAtPeriscope;
        MaxReverseSpeedAtMaxDepth = maneuverability.ManeuverabilitySubsMaxReverseSpeedAtMaxDepth;
        MaxDiveSpeed = maneuverability.ManeuverabilitySubsMaxDiveSpeed;
        DivingPlaneShiftTime = maneuverability.ManeuverabilitySubsDivingPlaneShiftTime;
        RudderShiftTime = maneuverability.ManeuverabilityRudderShiftTime;
        TurningCircle = maneuverability.ManeuverabilityTurningCircle;
        TimeToFullAhead = maneuverability.ForwardMaxSpeedTime;
        TimeToFullReverse = maneuverability.ReverseMaxSpeedTime;
        RudderProtection = maneuverability.RudderBlastProtection;
        EngineProtection = maneuverability.EngineBlastProtection;
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
