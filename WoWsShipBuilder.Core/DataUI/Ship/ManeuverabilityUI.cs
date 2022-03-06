using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ManeuverabilityUI : IDataUi
    {
        [DataUiUnit("Knots")]
        public decimal ManeuverabilityMaxSpeed { get; set; }

        [JsonIgnore]
        public string ManeuverabilityFullPowerForward { get; set; } = default!;

        [JsonIgnore]
        public string ManeuverabilityFullPowerBackward { get; set; } = default!;

        [DataUiUnit("M")]
        public decimal ManeuverabilityTurningCircle { get; set; }

        [DataUiUnit("S")]
        public decimal ManeuverabilityRudderShiftTime { get; set; }

        [JsonIgnore]
        public decimal RudderBlastProtection { get; set; }

        [JsonIgnore]
        public decimal EngineBlastProtection { get; set; }

        [JsonIgnore]
        public List<KeyValuePair<string, string>>? ManeuverabilityData { get; set; }

        public static ManeuverabilityUI FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Key, float Value)> modifiers)
        {
            var hull = ship.Hulls[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()];

            var engine = ship.Engines[shipConfiguration.First(upgrade => upgrade.UcType == ComponentType.Engine).Components[ComponentType.Engine].First()];

            decimal maxSpeedModifier = modifiers.FindModifiers("speedCoef", true).Aggregate(1m, (current, modifier) => current * (decimal)modifier);

            maxSpeedModifier = modifiers.FindModifiers("shipSpeedCoeff", true).Aggregate(maxSpeedModifier, (current, modifier) => current * (decimal)modifier);

            decimal rudderShiftModifier = modifiers.FindModifiers("SGRudderTime").Aggregate(1m, (current, modifier) => current * (decimal)modifier);

            decimal fullPowerForwardModifier = modifiers.FindModifiers("engineForwardUpTime").Aggregate(1m, (current, modifier) => current * (decimal)modifier);

            decimal fullPowerBackwardModifier = modifiers.FindModifiers("engineBackwardUpTime").Aggregate(1m, (current, modifier) => current * (decimal)modifier);

            var manouvrability = new ManeuverabilityUI
            {
                ManeuverabilityFullPowerBackward = $"{engine.BackwardEngineUpTime * fullPowerBackwardModifier} {Translation.Unit_S}",
                ManeuverabilityFullPowerForward = $"{engine.ForwardEngineUpTime * fullPowerForwardModifier} {Translation.Unit_S}",
                ManeuverabilityMaxSpeed = Math.Round(hull.MaxSpeed * (engine.SpeedCoef + 1) * maxSpeedModifier, 2),
                ManeuverabilityRudderShiftTime = Math.Round((hull.RudderTime * rudderShiftModifier) / 1.305M, 2),
                ManeuverabilityTurningCircle = hull.TurningRadius,
                RudderBlastProtection = hull.SteeringGearArmorCoeff,
                EngineBlastProtection = engine.ArmorCoeff,
            };

            manouvrability.ManeuverabilityData = manouvrability.ToPropertyMapping();

            return manouvrability;
        }
    }
}
