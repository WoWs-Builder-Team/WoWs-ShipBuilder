using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record TorpedoArmamentUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        [DataUiUnit("S")]
        public decimal TurnTime { get; set; }

        [DataUiUnit("DegreePerSecond")]
        public decimal TraverseSpeed { get; set; }

        [DataUiUnit("S")]
        public decimal Reload { get; set; }

        public string TorpedoArea { get; set; } = default!;

        [DataUiUnit("S")]
        public decimal TimeToSwitch { get; set; }

        [JsonIgnore]
        public List<TorpedoUI> Torpedoes { get; set; } = new();

        [JsonIgnore]
        public List<KeyValuePair<string, string>> TorpedoArmamentData { get; set; } = default!;

        public static TorpedoArmamentUI? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
        {
            ShipUpgrade? torpConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Torpedoes);
            if (torpConfiguration == null)
            {
                return null;
            }

            var torpedoModule = ship.TorpedoModules[torpConfiguration.Components[ComponentType.Torpedoes].First()];
            var launcher = torpedoModule.TorpedoLaunchers.First();

            List<(int BarrelCount, int TorpCount)> arrangementList = torpedoModule.TorpedoLaunchers
                .GroupBy(torpModule => torpModule.NumBarrels)
                .Select(group => (BarrelCount: group.Key, TorpCount: group.Count()))
                .OrderBy(item => item.TorpCount)
                .ToList();
            string torpArrangement = string.Join($"\n", arrangementList.Select(item => $"{item.TorpCount} x {item.BarrelCount} {Localizer.Instance[launcher.Name].Localization}"));

            var turnSpeedModifiers = modifiers.FindModifiers("GTRotationSpeed");
            decimal traverseSpeed = Math.Round(turnSpeedModifiers.Aggregate(launcher.HorizontalRotationSpeed, (current, modifier) => current * (decimal)modifier), 2);

            var reloadSpeedModifiers = modifiers.FindModifiers("GTShotDelay");
            decimal reloadSpeed = Math.Round(reloadSpeedModifiers.Aggregate(launcher.Reload, (current, modifier) => current * (decimal)modifier), 2);

            var arModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
            reloadSpeed = Math.Round(arModifiers.Aggregate(reloadSpeed, (current, arModifier) => current * (1 - ((decimal)arModifier / 100))), 2);

            var talentModifiers = modifiers.FindModifiers("torpedoReloadCoeff");
            reloadSpeed = Math.Round(talentModifiers.Aggregate(reloadSpeed, (current, modifier) => current * (decimal)modifier), 2);

            var torpedoArea = $"{launcher.TorpedoAngles[0]}° - {launcher.TorpedoAngles[1]}°";

            var torpedoes = TorpedoUI.FromTorpedoName(launcher.AmmoList, modifiers);

            var torpedoUi = new TorpedoArmamentUI
            {
                Name = torpArrangement,
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = traverseSpeed,
                Reload = reloadSpeed,
                TorpedoArea = torpedoArea,
                Torpedoes = torpedoes,
                TimeToSwitch = Math.Round(torpedoModule.TimeToChangeAmmo,  1),
            };

            torpedoUi.Torpedoes.Last().IsLast = true;

            torpedoUi.TorpedoArmamentData = torpedoUi.ToPropertyMapping();
            return torpedoUi;
        }
    }
}
