using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ManeuverabilityUI : IDataUi
    {
        [DataUiUnit("Knots")]
        public decimal ManeuverabilityMaxSpeed { get; set; }

        [DataUiUnit("S")]
        public decimal ManeuverabilityFullPowerForward { get; set; }

        [DataUiUnit("S")]
        public decimal ManeuverabilityFullPowerBackward { get; set; }

        [DataUiUnit("M")]
        public decimal ManeuverabilityTurningCircle { get; set; }

        [DataUiUnit("S")]
        public decimal ManeuverabilityRudderShiftTime { get; set; }

        [JsonIgnore]
        public List<KeyValuePair<string, string>>? ManeuverabilityData { get; set; }

        public static ManeuverabilityUI FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers)
        {
            var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];
            var engine = ship.Engines[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Engine).Components[ComponentType.Engine].First()];

            decimal maxSpeedModifier = 1;
            int maxSpeedIndex = modifiers.FindModifierIndex("speedCoef");
            if (maxSpeedIndex > -1)
            {
                var modifiersValues = modifiers.FindModifiers("speedCoef").ToList();
                foreach (decimal value in modifiersValues)
                {
                    maxSpeedModifier *= value;
                }
            }

            decimal rudderShiftModifier = 1;
            int rudderShiftIndex = modifiers.FindModifierIndex("SGRudderTime");
            if (rudderShiftIndex > -1)
            {
                var modifiersValues = modifiers.FindModifiers("SGRudderTime").ToList();
                foreach (decimal value in modifiersValues)
                {
                    rudderShiftModifier *= value;
                }
            }

            decimal fullPowerForwardModifier = 1;
            int fullPowerForwardIndex = modifiers.FindModifierIndex("engineForwardUpTime");
            if (rudderShiftIndex > -1)
            {
                var modifiersValues = modifiers.FindModifiers("engineForwardUpTime").ToList();
                foreach (decimal value in modifiersValues)
                {
                    fullPowerForwardModifier *= value;
                }
            }

            decimal fullPowerBackwardModifier = 1;
            int fullPowerBackwardIndex = modifiers.FindModifierIndex("engineBackwardUpTime");
            if (rudderShiftIndex > -1)
            {
                var modifiersValues = modifiers.FindModifiers("engineBackwardUpTime").ToList();
                foreach (decimal value in modifiersValues)
                {
                    fullPowerBackwardModifier *= value;
                }
            }

            var manouvrability = new ManeuverabilityUI
            {
                ManeuverabilityFullPowerBackward = engine.BackwardEngineUpTime * fullPowerBackwardModifier,
                ManeuverabilityFullPowerForward = engine.ForwardEngineUpTime * fullPowerForwardModifier,
                ManeuverabilityMaxSpeed = hull.MaxSpeed * (engine.SpeedCoef + 1) * maxSpeedModifier,
                ManeuverabilityRudderShiftTime = hull.RudderTime * rudderShiftModifier,
                ManeuverabilityTurningCircle = hull.TurningRadius,
            };

            manouvrability.ManeuverabilityData = manouvrability.ToPropertyMapping();

            return manouvrability;
        }
    }
}
