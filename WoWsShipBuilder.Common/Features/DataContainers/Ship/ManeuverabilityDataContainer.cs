using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial record ManeuverabilityDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots")]
    public decimal ManeuverabilityMaxSpeed { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots", LocalizationKeyOverride = "MaxReverseSpeed")]
    public decimal ManeuverabilityMaxReverseSpeed { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeed", UnitKey = "Knots")]
    public decimal ManeuverabilitySubsMaxSpeedOnSurface { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeed", UnitKey = "Knots")]
    public decimal ManeuverabilitySubsMaxSpeedAtPeriscope { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeed", UnitKey = "Knots")]
    public decimal ManeuverabilitySubsMaxSpeedAtMaxDepth { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxReverseSpeed", UnitKey = "Knots", LocalizationKeyOverride = "ManeuverabilitySubsMaxSpeedOnSurface")]
    public decimal ManeuverabilitySubsMaxReverseSpeedOnSurface { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxReverseSpeed", UnitKey = "Knots", LocalizationKeyOverride = "ManeuverabilitySubsMaxSpeedAtPeriscope")]
    public decimal ManeuverabilitySubsMaxReverseSpeedAtPeriscope { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxReverseSpeed", UnitKey = "Knots", LocalizationKeyOverride = "ManeuverabilitySubsMaxSpeedAtMaxDepth")]
    public decimal ManeuverabilitySubsMaxReverseSpeedAtMaxDepth { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MPS")]
    public decimal ManeuverabilitySubsMaxDiveSpeed { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ManeuverabilitySubsDivingPlaneShiftTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ManeuverabilityRudderShiftTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
    public decimal ManeuverabilityTurningCircle { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeedTime", UnitKey = "S")]
    public decimal ForwardMaxSpeedTime { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeedTime", UnitKey = "S")]
    public decimal ReverseMaxSpeedTime { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "BlastProtection", TooltipKey = "BlastExplanation")]
    [DataElementFiltering(false)]
    public decimal RudderBlastProtection { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "BlastProtection", TooltipKey = "BlastExplanation")]
    [DataElementFiltering(false)]
    public decimal EngineBlastProtection { get; set; }

    public static ManeuverabilityDataContainer FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<Modifier> modifiers)
    {
        var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull][0]];

        var engine = ship.Engines[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Engine).Components[ComponentType.Engine][0]];

        decimal maxSpeedModifier = modifiers.ApplyModifiers("ManeuverabilityDataContainer.Speed", 1m);

        var speedBoostModifier = modifiers.ApplyModifiers("ManeuverabilityDataContainer.BoostCoeff.SpeedBoost", 0);
        if (speedBoostModifier != 0)
        {
            maxSpeedModifier += speedBoostModifier + modifiers.ApplyModifiers("ManeuverabilityDataContainer.SpeedBoostForsage", 0); // Speed boost is additive, Halland UU bonus only applies if regular speed boost is active
        }

        decimal maxDiveSpeedModifier = modifiers.ApplyModifiers("ManeuverabilityDataContainer.MaxDiveSpeed", 1m);

        decimal divingPlaneShiftTimeModifier = modifiers.ApplyModifiers("ManeuverabilityDataContainer.DivingPlaneShiftTime", 1m);

        decimal enlargedPropellerShaftSpeedModifier = modifiers.ApplyModifiers("ManeuverabilityDataContainer.PropellerShaftSpeed", 1m);

        decimal rudderShiftModifier = modifiers.ApplyModifiers("ManeuverabilityDataContainer.RudderShiftTime", 1m);

        double engineForwardUpTimeModifiers = (double)modifiers.ApplyModifiers("ManeuverabilityDataContainer.EngineForwardUpTime", 1m);
        double engineBackwardUpTimeModifiers = (double)modifiers.ApplyModifiers("ManeuverabilityDataContainer.EngineBackwardUpTime", 1m);
        double engineForwardForsageMaxSpeedModifier = (double)modifiers.ApplyModifiers("ManeuverabilityDataContainer.EngineForwardForsageMaxSpeed", 1m);
        double engineBackwardForsageMaxSpeedModifier = (double)modifiers.ApplyModifiers("ManeuverabilityDataContainer.EngineBackwardForsageMaxSpeed", 1m);
        double engineForwardForsagePowerModifier = (double)modifiers.ApplyModifiers("ManeuverabilityDataContainer.EngineForwardForsagePower", 1m);
        double engineBackwardForsagePowerModifier = (double)modifiers.ApplyModifiers("ManeuverabilityDataContainer.EngineBackwardForsagePower", 1m);

        var accelerationModifiers = new AccelerationCalculator.AccelerationModifiers((double)(maxSpeedModifier * enlargedPropellerShaftSpeedModifier), engineForwardUpTimeModifiers, engineBackwardUpTimeModifiers, engineForwardForsageMaxSpeedModifier, engineBackwardForsageMaxSpeedModifier, engineForwardForsagePowerModifier, engineBackwardForsagePowerModifier);

        // speed boost overrides
        var speedBoostForwardEngineForsagMaxSpeedOverride = (double)modifiers.ApplyModifiers("ManeuverabilityDataContainer.ForwardEngineForsagMaxSpeed.SpeedBoost", 0m);
        var speedBoostBackwardEngineForsagMaxSpeedOverride = (double)modifiers.ApplyModifiers("ManeuverabilityDataContainer.BackwardEngineForsagMaxSpeed.SpeedBoost", 0m);
        var speedBoostForwardEngineForsagOverride = (double)modifiers.ApplyModifiers("ManeuverabilityDataContainer.ForwardEngineForsag.SpeedBoost", 0m);
        var speedBoostBackwardEngineForsagOverride = (double)modifiers.ApplyModifiers("ManeuverabilityDataContainer.BackwardEngineForsag.SpeedBoost", 0m);

        var speedBoostAccelerationModifiers = new AccelerationCalculator.SpeedBoostAccelerationModifiers(speedBoostForwardEngineForsagMaxSpeedOverride, speedBoostBackwardEngineForsagMaxSpeedOverride, speedBoostForwardEngineForsagOverride, speedBoostBackwardEngineForsagOverride);

        List<int> forward = new() { AccelerationCalculator.Zero, AccelerationCalculator.FullAhead };
        List<int> reverse = new() { AccelerationCalculator.Zero, AccelerationCalculator.FullReverse };

        var timeForward = AccelerationCalculator.CalculateAcceleration(ship.Index, hull, engine, ship.ShipClass, forward, accelerationModifiers, speedBoostAccelerationModifiers).TimeForGear.Single();
        var timeBackward = AccelerationCalculator.CalculateAcceleration(ship.Index, hull, engine, ship.ShipClass, reverse, accelerationModifiers, speedBoostAccelerationModifiers).TimeForGear.Single();

        hull.MaxSpeedAtBuoyancyStateCoeff.TryGetValue(SubmarineBuoyancyStates.Periscope, out decimal speedAtPeriscopeCoeff);
        hull.MaxSpeedAtBuoyancyStateCoeff.TryGetValue(SubmarineBuoyancyStates.DeepWater, out decimal speedAtMaxDepthCoeff);

        decimal baseShipSpeed = hull.MaxSpeed * (engine.SpeedCoef + 1);
        decimal maxSpeed = baseShipSpeed * maxSpeedModifier;
        decimal maxSpeedOnSurface = 0;
        decimal maxSpeedAtPeriscope = 0;
        decimal maxSpeedAtMaxDepth = 0;
        decimal maxDiveSpeed = 0;
        decimal maxReverseSpeed = ((baseShipSpeed / 4) + 4.9m) * maxSpeedModifier;
        decimal maxReverseSpeedOnSurface = 0;
        decimal maxReverseSpeedAtPeriscope = 0;
        decimal maxReverseSpeedAtMaxDepth = 0;

        decimal divingPlaneShiftTime = 0;

        if (ship.ShipClass == ShipClass.Submarine)
        {
            maxSpeedOnSurface = maxSpeed * enlargedPropellerShaftSpeedModifier;
            maxSpeedAtPeriscope = maxSpeed * speedAtPeriscopeCoeff * enlargedPropellerShaftSpeedModifier;
            maxSpeedAtMaxDepth = maxSpeed * speedAtMaxDepthCoeff;
            maxReverseSpeedOnSurface = maxReverseSpeed * enlargedPropellerShaftSpeedModifier;
            maxReverseSpeedAtPeriscope = maxReverseSpeed * speedAtPeriscopeCoeff * enlargedPropellerShaftSpeedModifier;
            maxReverseSpeedAtMaxDepth = maxReverseSpeed * speedAtMaxDepthCoeff;
            maxSpeed = 0;
            maxReverseSpeed = 0;
            maxDiveSpeed = hull.DiveSpeed;

            divingPlaneShiftTime = hull.DivingPlaneShiftTime;
        }

        var manoeuvrability = new ManeuverabilityDataContainer
        {
            ForwardMaxSpeedTime = Math.Round((decimal)timeForward, 2),
            ReverseMaxSpeedTime = Math.Round((decimal)timeBackward, 2),
            ManeuverabilityMaxSpeed = Math.Round(maxSpeed, 2),
            ManeuverabilityMaxReverseSpeed = Math.Round(maxReverseSpeed, 2),
            ManeuverabilitySubsMaxDiveSpeed = Math.Round(maxDiveSpeed * maxDiveSpeedModifier, 2),
            ManeuverabilitySubsMaxSpeedOnSurface = Math.Round(maxSpeedOnSurface, 2),
            ManeuverabilitySubsMaxSpeedAtPeriscope = Math.Round(maxSpeedAtPeriscope, 2),
            ManeuverabilitySubsMaxSpeedAtMaxDepth = Math.Round(maxSpeedAtMaxDepth, 2),
            ManeuverabilitySubsMaxReverseSpeedOnSurface = Math.Round(maxReverseSpeedOnSurface, 2),
            ManeuverabilitySubsMaxReverseSpeedAtPeriscope = Math.Round(maxReverseSpeedAtPeriscope, 2),
            ManeuverabilitySubsMaxReverseSpeedAtMaxDepth = Math.Round(maxReverseSpeedAtMaxDepth, 2),
            ManeuverabilityRudderShiftTime = Math.Round((hull.RudderTime * rudderShiftModifier) / 1.305M, 2),
            ManeuverabilitySubsDivingPlaneShiftTime = Math.Round(divingPlaneShiftTime * divingPlaneShiftTimeModifier, 2),
            ManeuverabilityTurningCircle = hull.TurningRadius,
            RudderBlastProtection = hull.SteeringGearArmorCoeff,
            EngineBlastProtection = engine.ArmorCoeff,
        };

        manoeuvrability.UpdateDataElements();

        return manoeuvrability;
    }
}
