using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Core.DataContainers;

public partial record ManeuverabilityDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots")]
    public decimal ManeuverabilityMaxSpeed { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeed", UnitKey = "Knots")]
    public decimal ManeuverabilitySubsMaxSpeedOnSurface { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeed", UnitKey = "Knots")]
    public decimal ManeuverabilitySubsMaxSpeedAtPeriscope { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "MaxSpeed", UnitKey = "Knots")]
    public decimal ManeuverabilitySubsMaxSpeedAtMaxDepth { get; set; }

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
        maxSpeedModifier = modifiers.FindModifiers("boostCoeff").Aggregate(maxSpeedModifier, (current, modifier) => current + (decimal)modifier); // Speed boost is additive

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
        var accelerationModifiers = new AccelerationHelper.AccelerationModifiers((double)maxSpeedModifier, engineForwardUpTimeModifiers, engineBackwardUpTimeModifiers, engineForwardForsageMaxSpeedModifier, engineBackwardForsageMaxSpeedModifier, engineForwardForsagePowerModifier, engineBackwardForsagePowerModifier);

        // speed boost overrides
        var speedBoostEngineForwardForsageMaxSpeedOverride = modifiers.FindModifiers("speedBoost_engineForwardForsageMaxSpeed", true).FirstOrDefault(0);
        var speedBoostEngineBackwardEngineForsagOverride = modifiers.FindModifiers("speedBoost_backwardEngineForsagMaxSpeed", true).FirstOrDefault(0);
        var speedBoostForwardEngineForsagOverride = modifiers.FindModifiers("speedBoost_forwardEngineForsag", true).FirstOrDefault(0);
        var speedBoostBackwardEngineForsag = modifiers.FindModifiers("speedBoost_backwardEngineForsag", true).FirstOrDefault(0);
        var speedBoostAccelerationModifiers = new AccelerationHelper.SpeedBoostAccelerationModifiers(speedBoostEngineForwardForsageMaxSpeedOverride, speedBoostEngineBackwardEngineForsagOverride, speedBoostForwardEngineForsagOverride, speedBoostBackwardEngineForsag);

        List<int> forward = new List<int> { AccelerationHelper.Zero, AccelerationHelper.FullAhead };
        List<int> reverse = new List<int> { AccelerationHelper.Zero, AccelerationHelper.FullReverse };

        var timeForward = AccelerationHelper.CalculateAcceleration(ship.Index, hull, engine, ship.ShipClass, forward, accelerationModifiers, speedBoostAccelerationModifiers).TimeForGear.Single();
        var timeBackward = AccelerationHelper.CalculateAcceleration(ship.Index, hull, engine, ship.ShipClass, reverse, accelerationModifiers, speedBoostAccelerationModifiers).TimeForGear.Single();

        hull.MaxSpeedAtBuoyancyStateCoeff.TryGetValue(SubsBuoyancyStates.Periscope, out decimal speedAtPeriscopeCoeff);
        hull.MaxSpeedAtBuoyancyStateCoeff.TryGetValue(SubsBuoyancyStates.DeepWater, out decimal speedAtMaxDepthCoeff);

        decimal maxSpeed = hull.MaxSpeed * (engine.SpeedCoef + 1) * maxSpeedModifier;
        decimal maxSpeedOnSurface = 0;
        decimal maxSpeedAtPeriscope = 0;
        decimal maxSpeedAtMaxDepth = 0;
        decimal maxDiveSpeed = 0;

        decimal divingPlaneShiftTime = 0;

        if (ship.ShipClass == ShipClass.Submarine)
        {
            maxSpeedOnSurface = maxSpeed * enlargedPropellerShaftSpeedModifier;
            maxSpeedAtPeriscope = maxSpeed * speedAtPeriscopeCoeff * enlargedPropellerShaftSpeedModifier;
            maxSpeedAtMaxDepth = maxSpeed * speedAtMaxDepthCoeff;
            maxSpeed = 0;
            maxDiveSpeed = hull.DiveSpeed;

            divingPlaneShiftTime = hull.DivingPlaneShiftTime;
        }

        var manoeuvrability = new ManeuverabilityDataContainer
        {
            ForwardMaxSpeedTime = Math.Round((decimal)timeForward, 2),
            ReverseMaxSpeedTime = Math.Round((decimal)timeBackward, 2),
            ManeuverabilityMaxSpeed = Math.Round(maxSpeed, 2),
            ManeuverabilitySubsMaxDiveSpeed = Math.Round(maxDiveSpeed * maxDiveSpeedModifier, 2),
            ManeuverabilitySubsMaxSpeedOnSurface = Math.Round(maxSpeedOnSurface, 2),
            ManeuverabilitySubsMaxSpeedAtPeriscope = Math.Round(maxSpeedAtPeriscope, 2),
            ManeuverabilitySubsMaxSpeedAtMaxDepth = Math.Round(maxSpeedAtMaxDepth, 2),
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
