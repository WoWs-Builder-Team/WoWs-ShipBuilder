using WoWsShipBuilder.Core.DataContainers;

namespace WoWsShipBuilder.ViewModels.Helper.GridData;

public class ManeuverabilityGridDataWrapper
{
    public ManeuverabilityGridDataWrapper(ManeuverabilityDataContainer maneuverability)
    {
        MaxSpeed = maneuverability.ManeuverabilityMaxSpeed != 0 ? maneuverability.ManeuverabilityMaxSpeed : maneuverability.ManeuverabilitySubsMaxSpeedOnSurface;
        MaxSpeedAtPeriscopeDepth = maneuverability.ManeuverabilitySubsMaxSpeedAtPeriscope;
        MaxSpeedAtMaxDepth = maneuverability.ManeuverabilitySubsMaxSpeedAtMaxDepth;
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

    public decimal RudderShiftTime { get; }

    public decimal TurningCircle { get; }

    public decimal TimeToFullAhead { get; }

    public decimal TimeToFullReverse { get; }

    public decimal RudderProtection { get; }

    public decimal EngineProtection { get; }
}
