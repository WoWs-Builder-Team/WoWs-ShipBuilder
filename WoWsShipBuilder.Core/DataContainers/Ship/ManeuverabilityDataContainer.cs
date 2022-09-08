using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataContainers;

public partial record ManeuverabilityDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots")]
    public decimal ManeuverabilityMaxSpeed { get; set; }

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
        maxSpeedModifier = modifiers.FindModifiers("boostCoeff", true).Aggregate(maxSpeedModifier, (current, modifier) => current * ((decimal)modifier + 1));

        decimal rudderShiftModifier = modifiers.FindModifiers("SGRudderTime").Aggregate(1m, (current, modifier) => current * (decimal)modifier);

        // decimal engineForwardUpTimeModifier = modifiers.FindModifiers("engineForwardUpTime").Aggregate(1m, (current, modifier) => current * (decimal)modifier);
        //
        // decimal engineBackwardUpTimeModifier = modifiers.FindModifiers("engineBackwardUpTime").Aggregate(1m, (current, modifier) => current * (decimal)modifier);

        var engineForwardUpTimeModifiers = modifiers.Where(x => x.Key.Equals("engineForwardUpTime")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var engineBackwardUpTimeModifiers = modifiers.Where(x => x.Key.Equals("engineBackwardUpTime")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var engineForwardForsageMaxSpeedModifier = modifiers.Where(x => x.Key.Equals("engineForwardForsageMaxSpeed")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var engineBackwardForsageMaxSpeedModifier = modifiers.Where(x => x.Key.Equals("engineBackwardForsageMaxSpeed")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var engineForwardForsagePowerModifier = modifiers.Where(x => x.Key.Equals("engineForwardForsagePower")).Aggregate(1d, (current, modifier) => current * modifier.Value);
        var engineBackwardForsagePowerModifier = modifiers.Where(x => x.Key.Equals("engineBackwardForsagePower")).Aggregate(1d, (current, modifier) => current * modifier.Value);

        List<int> forward = new List<int>() {AccelerationHelper.Zero, AccelerationHelper.FullAhead };
        List<int> reverse = new List<int>() {AccelerationHelper.Zero, AccelerationHelper.FullReverse};

        var timeForward = AccelerationHelper.CalculateAcceleration(ship.Index, hull, engine, ship.ShipClass, forward, (double)maxSpeedModifier, engineForwardUpTimeModifiers, engineBackwardUpTimeModifiers,
            engineForwardForsageMaxSpeedModifier, engineBackwardForsageMaxSpeedModifier, engineForwardForsagePowerModifier, engineBackwardForsagePowerModifier).TimeForGear.Single();

        var timeBackward = AccelerationHelper.CalculateAcceleration(ship.Index, hull, engine, ship.ShipClass, reverse, (double)maxSpeedModifier, engineForwardUpTimeModifiers, engineBackwardUpTimeModifiers,
            engineForwardForsageMaxSpeedModifier, engineBackwardForsageMaxSpeedModifier, engineForwardForsagePowerModifier, engineBackwardForsagePowerModifier).TimeForGear.Single();

        var manoeuvrability = new ManeuverabilityDataContainer
        {
            ForwardMaxSpeedTime = Math.Round((decimal)timeForward, 1),
            ReverseMaxSpeedTime = Math.Round((decimal)timeBackward, 1),
            ManeuverabilityMaxSpeed = Math.Round(hull.MaxSpeed * (engine.SpeedCoef + 1) * maxSpeedModifier, 2),
            ManeuverabilityRudderShiftTime = Math.Round((hull.RudderTime * rudderShiftModifier) / 1.305M, 2),
            ManeuverabilityTurningCircle = hull.TurningRadius,
            RudderBlastProtection = hull.SteeringGearArmorCoeff,
            EngineBlastProtection = engine.ArmorCoeff,
        };

        manoeuvrability.UpdateDataElements();

        return manoeuvrability;
    }
}
