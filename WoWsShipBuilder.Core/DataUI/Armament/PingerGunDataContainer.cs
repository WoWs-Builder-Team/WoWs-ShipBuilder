using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
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

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal FirstPingDuration { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal SecondPingDuration { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
        public decimal PingWidth { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MPS")]
        public decimal PingSpeed { get; set; }

        public static PingerGunDataContainer? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
        {
            var pingerGunList = ship.PingerGunList;

            if (pingerGunList is null)
            {
                return null;
            }

            var pingerGun = pingerGunList.First().Value;

            var pingSpeed = pingerGun.WaveParams.First().WaveSpeed.First();
            var pingSpeedModifiers = modifiers.FindModifiers("pingerWaveSpeedCoeff");
            pingSpeed = pingSpeedModifiers.Aggregate(pingSpeed, (current, reloadModifier) => current * (decimal)reloadModifier);

            var firstPingDuration = pingerGun.SectorParams[0].Lifetime;
            var firstPingDurationModifiers = modifiers.FindModifiers("firstSectorTimeCoeff");
            firstPingDuration = firstPingDurationModifiers.Aggregate(firstPingDuration, (current, reloadModifier) => current * (decimal)reloadModifier);

            var secondPingDuration = pingerGun.WaveParams.First().WaveSpeed.First();
            var secondPingDurationModifiers = modifiers.FindModifiers("secondSectorTimeCoeff");
            secondPingDuration = secondPingDurationModifiers.Aggregate(secondPingDuration, (current, reloadModifier) => current * (decimal)reloadModifier);

            var traverseSpeed = pingerGun.RotationSpeed[0];

            var arModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
            var reload = arModifiers.Aggregate(pingerGun.WaveReloadTime, (current, arModifier) => current * (1 - ((decimal)arModifier / 100)));

            var pingerGunDataContainer = new PingerGunDataContainer
            {
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = traverseSpeed,
                Reload = Math.Round(reload, 2),
                Range = pingerGun.WaveDistance / 1000,
                FirstPingDuration = Math.Round(firstPingDuration, 1),
                SecondPingDuration = Math.Round(secondPingDuration, 1),
                PingWidth = pingerGun.WaveParams.First().StartWaveWidth * 30,
                PingSpeed = Math.Round(pingSpeed, 0),
            };

            pingerGunDataContainer.UpdateDataElements();

            return pingerGunDataContainer;
        }
    }
}
