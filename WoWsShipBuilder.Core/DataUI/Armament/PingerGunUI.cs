using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataUI.Projectiles;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record PingerGunUI : IDataUi
    {
        [DataUiUnit("S")]
        public decimal Reload { get; set; }

        [DataUiUnit("DegreePerSecond")]
        public decimal TraverseSpeed { get; set; }

        [DataUiUnit("S")]
        public decimal TurnTime { get; set; }

        [DataUiUnit("KM")]
        public decimal Range { get; set; }

        [DataUiUnit("S")]
        public decimal FirstPingDuration { get; set; }

        [DataUiUnit("S")]
        public decimal SecondPingDuration { get; set; }

        [DataUiUnit("M")]
        public decimal PingWidth { get; set; }

        [DataUiUnit("MPS")]
        public decimal PingSpeed { get; set; }

        [JsonIgnore]
        public List<KeyValuePair<string, string>>? PingerGunData { get; set; }

        public static PingerGunUI? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
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

            var pingerGunUI = new PingerGunUI
            {
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = traverseSpeed,
                Reload = pingerGun.WaveReloadTime,
                Range = pingerGun.WaveDistance,
                FirstPingDuration = firstPingDuration,
                SecondPingDuration = secondPingDuration,
                PingWidth = pingerGun.WaveParams.First().StartWaveWidth * 30,
                PingSpeed = pingSpeed,
            };

            pingerGunUI.PingerGunData = pingerGunUI.ToPropertyMapping();

            return pingerGunUI;
        }
    }
}
