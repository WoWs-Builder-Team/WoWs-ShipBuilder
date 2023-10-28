using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.Utility;

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

    public static ManeuverabilityDataContainer FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers)
    {
        var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];

        var engine = ship.Engines[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Engine).Components[ComponentType.Engine].First()];

        decimal maxSpeedModifier = modifiers.FindModifiers("speedCoef", true).Aggregate(1m, (current, modifier) => current * (decimal)modifier);
        maxSpeedModifier = modifiers.FindModifiers("shipSpeedCoeff", true).Aggregate(maxSpeedModifier, (current, modifier) => current * (decimal)modifier);

        var speedBoostModifier = modifiers.FindModifiers("speedBoost_boostCoeff", true).Sum();
        if (speedBoostModifier != 0)
        {
            maxSpeedModifier += (decimal)(speedBoostModifier + modifiers.FindModifiers("boostCoeffForsage").Sum()); // Speed boost is additive, Halland UU bonus only applies if regular speed boost is active
        }

        decimal maxDiveSpeedModifier = modifiers.FindModifiers("maxBuoyancySpeedCoeff", true).Aggregate(1m, (current, modifier) => current * (decimal)modifier);

        decimal divingPlaneShiftTimeModifier = modifiers.FindModifiers("buoyancyRudderTimeCoeff", true).Aggregate(1m, (current, modifier) => current * (decimal)modifier);

        decimal enlargedPropellerShaftSpeedModifier = modifiers.FindModifiers("speedCoefBattery", true).Aggregate(1m, (current, modifier) => current * (decimal)modifier);

        decimal rudderShiftModifier = modifiers.FindModifiers("SGRudderTime").Aggregate(1m, (current, modifier) => current * (decimal)modifier);

        var engineForwardUpTimeModifiers = modifiers.Where(x => x.Key.Equals("engineForwardUpTime")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var engineBackwardUpTimeModifiers = modifiers.Where(x => x.Key.Equals("engineBackwardUpTime")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var engineForwardForsageMaxSpeedModifier = modifiers.Where(x => x.Key.Equals("engineForwardForsageMaxSpeed")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var engineBackwardForsageMaxSpeedModifier = modifiers.Where(x => x.Key.Equals("engineBackwardForsageMaxSpeed")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var engineForwardForsagePowerModifier = modifiers.Where(x => x.Key.Equals("engineForwardForsagePower")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var engineBackwardForsagePowerModifier = modifiers.Where(x => x.Key.Equals("engineBackwardForsagePower")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var accelerationModifiers = new AccelerationCalculator.AccelerationModifiers((double)maxSpeedModifier, engineForwardUpTimeModifiers, engineBackwardUpTimeModifiers, engineForwardForsageMaxSpeedModifier, engineBackwardForsageMaxSpeedModifier, engineForwardForsagePowerModifier, engineBackwardForsagePowerModifier);

        // speed boost overrides
        var speedBoostEngineForwardForsageMaxSpeedOverride = modifiers.FindModifiers("speedBoost_engineForwardForsageMaxSpeed", true).FirstOrDefault(0);
        var speedBoostEngineBackwardEngineForsagOverride = modifiers.FindModifiers("speedBoost_backwardEngineForsagMaxSpeed", true).FirstOrDefault(0);
        var speedBoostForwardEngineForsagOverride = modifiers.FindModifiers("speedBoost_forwardEngineForsag", true).FirstOrDefault(0);
        var speedBoostBackwardEngineForsag = modifiers.FindModifiers("speedBoost_backwardEngineForsag", true).FirstOrDefault(0);
        var speedBoostAccelerationModifiers = new AccelerationCalculator.SpeedBoostAccelerationModifiers(speedBoostEngineForwardForsageMaxSpeedOverride, speedBoostEngineBackwardEngineForsagOverride, speedBoostForwardEngineForsagOverride, speedBoostBackwardEngineForsag);

        List<int> forward = new() { AccelerationCalculator.Zero, AccelerationCalculator.FullAhead };
        List<int> reverse = new() { AccelerationCalculator.Zero, AccelerationCalculator.FullReverse };

        var timeForward = AccelerationCalculator.CalculateAcceleration(ship.Index, hull, engine, ship.ShipClass, forward, accelerationModifiers, speedBoostAccelerationModifiers).TimeForGear.Single();
        var timeBackward = AccelerationCalculator.CalculateAcceleration(ship.Index, hull, engine, ship.ShipClass, reverse, accelerationModifiers, speedBoostAccelerationModifiers).TimeForGear.Single();

        hull.MaxSpeedAtBuoyancyStateCoeff.TryGetValue(SubsBuoyancyStates.Periscope, out decimal speedAtPeriscopeCoeff);
        hull.MaxSpeedAtBuoyancyStateCoeff.TryGetValue(SubsBuoyancyStates.DeepWater, out decimal speedAtMaxDepthCoeff);

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
