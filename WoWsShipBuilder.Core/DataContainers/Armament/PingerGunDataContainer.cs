using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Core.DataContainers
{
    public partial record PingerGunDataContainer : DataContainerBase
    {
        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal Reload { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "DegreePerSecond")]
        public decimal TraverseSpeed { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal TurnTime { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
        public decimal Range { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "PingDuration", UnitKey = "S", NameLocalizationKey = "First")]
        public decimal FirstPingDuration { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "PingDuration", UnitKey = "S", NameLocalizationKey = "Second")]
        public decimal SecondPingDuration { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
        public decimal PingWidth { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MPS")]
        public decimal PingSpeed { get; set; }

        public static PingerGunDataContainer? FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
        {
            if (!ship.PingerGunList.Any())
            {
                return null;
            }

            PingerGun pingerGun;
            var pingerUpgrade = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Sonar);
            if (pingerUpgrade is null && ship.PingerGunList.Count is 1)
            {
                Logging.Logger.Warn("No sonar upgrade information found for ship {ShipName} even though there is one sonar module available.", ship.Name);
                return null;
            }

            if (pingerUpgrade is null)
            {
                throw new InvalidOperationException($"No sonar upgrade information found for ship {ship.Name} but there is more than one sonar module.");
            }

            // Safe approach is necessary because data up until 0.11.9#1 does not include this data due to an issue in the data converter
            if (pingerUpgrade.Components.TryGetValue(ComponentType.Sonar, out string[]? pingerGunInfo))
            {
                pingerGun = ship.PingerGunList[pingerGunInfo.First()];
            }
            else
            {
                Logging.Logger.Warn("Unable to retrieve sonar component from upgrade info for ship {} and ship upgrade {}", ship.Index, pingerUpgrade.Name);
                pingerGun = ship.PingerGunList.First().Value;
            }

            var pingSpeed = pingerGun.WaveParams.First().WaveSpeed.First();
            var pingSpeedModifiers = modifiers.FindModifiers("pingerWaveSpeedCoeff");
            pingSpeed = pingSpeedModifiers.Aggregate(pingSpeed, (current, pingSpeedModifier) => current * (decimal)pingSpeedModifier);

            var firstPingDuration = pingerGun.SectorParams[0].Lifetime;
            var firstPingDurationModifiers = modifiers.FindModifiers("firstSectorTimeCoeff");
            firstPingDuration = firstPingDurationModifiers.Aggregate(firstPingDuration, (current, firstPingDurationModifier) => current * (decimal)firstPingDurationModifier);

            var secondPingDuration = pingerGun.SectorParams[1].Lifetime;
            var secondPingDurationModifiers = modifiers.FindModifiers("secondSectorTimeCoeff");
            secondPingDuration = secondPingDurationModifiers.Aggregate(secondPingDuration, (current, secondPingDurationModifiersModifier) => current * (decimal)secondPingDurationModifiersModifier);

            var traverseSpeed = pingerGun.RotationSpeed[0];

            var arModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
            var reload = arModifiers.Aggregate(pingerGun.WaveReloadTime, (current, arModifier) => current * (1 - ((decimal)arModifier / 100)));
            var pingReloadModifiers = modifiers.FindModifiers("pingerReloadCoeff");
            reload = pingReloadModifiers.Aggregate(reload, (current, pingReloadModifier) => current * (decimal)pingReloadModifier);

            var pingerGunDataContainer = new PingerGunDataContainer
            {
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = traverseSpeed,
                Reload = Math.Round(reload, 2),
                Range = pingerGun.WaveDistance / 1000,
                FirstPingDuration = Math.Round(firstPingDuration, 1),
                SecondPingDuration = Math.Round(secondPingDuration, 1),
                PingWidth = pingerGun.WaveParams.First().StartWaveWidth,
                PingSpeed = Math.Round(pingSpeed, 0),
            };

            pingerGunDataContainer.UpdateDataElements();

            return pingerGunDataContainer;
        }
    }
}
