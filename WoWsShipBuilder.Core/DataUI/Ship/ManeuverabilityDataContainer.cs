using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public partial record ManeuverabilityDataContainer : DataContainerBase
    {
        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "FullPowerTime", UnitKey = "S")]
        public decimal FullPowerForward { get; set; } = default!;

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "FullPowerTime", UnitKey = "S")]
        public decimal FullPowerBackward { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots")]
        public decimal ManeuverabilityMaxSpeed { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
        public decimal ManeuverabilityTurningCircle { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal ManeuverabilityRudderShiftTime { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "BlastProtection", TooltipKey = "BlastExplanation")]
        [DataElementVisibility(false)]
        public decimal RudderBlastProtection { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "BlastProtection", TooltipKey = "BlastExplanation")]
        [DataElementVisibility(false)]
        public decimal EngineBlastProtection { get; set; }

        public static ManeuverabilityDataContainer FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers)
        {
            var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];

            var engine = ship.Engines[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Engine).Components[ComponentType.Engine].First()];

            decimal maxSpeedModifier = modifiers.FindModifiers("speedCoef", true).Aggregate(1m, (current, modifier) => current * (decimal)modifier);

            maxSpeedModifier = modifiers.FindModifiers("shipSpeedCoeff", true).Aggregate(maxSpeedModifier, (current, modifier) => current * (decimal)modifier);

            decimal rudderShiftModifier = modifiers.FindModifiers("SGRudderTime").Aggregate(1m, (current, modifier) => current * (decimal)modifier);

            decimal fullPowerForwardModifier = modifiers.FindModifiers("engineForwardUpTime").Aggregate(1m, (current, modifier) => current * (decimal)modifier);

            decimal fullPowerBackwardModifier = modifiers.FindModifiers("engineBackwardUpTime").Aggregate(1m, (current, modifier) => current * (decimal)modifier);

            var manoeuvrability = new ManeuverabilityDataContainer
            {
                FullPowerBackward = engine.BackwardEngineUpTime * fullPowerBackwardModifier,
                FullPowerForward = engine.ForwardEngineUpTime * fullPowerForwardModifier,
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
}
