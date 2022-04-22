using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
            pingSpeed = Math.Round(pingSpeedModifiers.Aggregate(pingSpeed, (current, reloadModifier) => current * (decimal)reloadModifier), 0);

            var firstPingDuration = pingerGun.SectorParams[0].Lifetime;
            var firstPingDurationModifiers = modifiers.FindModifiers("firstSectorTimeCoeff");
            firstPingDuration = Math.Round(pingSpeedModifiers.Aggregate(firstPingDuration, (current, reloadModifier) => current * (decimal)reloadModifier), 1);

            var secondPingDuration = pingerGun.WaveParams.First().WaveSpeed.First();
            var secondPingDurationModifiers = modifiers.FindModifiers("secondSectorTimeCoeff");
            secondPingDuration = Math.Round(secondPingDurationModifiers.Aggregate(secondPingDuration, (current, reloadModifier) => current * (decimal)reloadModifier), 1);

            var traverseSpeed = pingerGun.RotationSpeed[0];

            var arModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
            var reload = Math.Round(arModifiers.Aggregate(pingerGun.WaveReloadTime, (current, arModifier) => current * (1 - ((decimal)arModifier / 100))), 2);

            var pingerGunUI = new PingerGunDataContainer
            {
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = traverseSpeed,
                Reload = reload,
                Range = pingerGun.WaveDistance / 1000,
                FirstPingDuration = firstPingDuration,
                SecondPingDuration = secondPingDuration,
                PingWidth = pingerGun.WaveParams.First().StartWaveWidth * 30,
                PingSpeed = pingSpeed,
            };

            pingerGunUI.UpdateDataElements();
            //pingerGunUI.PingerGunData = pingerGunUI.ToPropertyMapping();

            return pingerGunUI;
        }

    }
}
