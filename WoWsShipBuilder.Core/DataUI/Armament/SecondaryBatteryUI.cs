using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SecondaryBatteryUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        [DataUiUnit("KM")]
        public decimal Range { get; set; }

        [DataUiUnit("S")]
        public decimal Reload { get; set; }

        [DataUiUnit("ShotsPerSecond")]
        public decimal RoF { get; set; }

        [JsonIgnore]
        public ShellUI? Shell { get; set; }

        [JsonIgnore]
        public bool IsLast { get; set; } = false;

        [JsonIgnore]
        public List<KeyValuePair<string, string>> SecondaryBatteryData { get; set; } = default!;

        public static List<SecondaryBatteryUI>? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
        {
            TurretModule? secondary = ship.Hulls[shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()]
                .SecondaryModule;
            if (secondary == null)
            {
                return null;
            }

            var result = new List<SecondaryBatteryUI>();
            List<List<Gun>> groupedSecondaries = secondary.Guns.GroupBy(gun => new { gun.BarrelDiameter, gun.NumBarrels })
                .OrderBy(group => group.Key.BarrelDiameter)
                .ThenBy(group => group.Key.NumBarrels)
                .Select(group => group.ToList())
                .ToList();

            foreach (List<Gun> secondaryGroup in groupedSecondaries)
            {
                var secondaryGun = secondaryGroup.First();

                var reloadModifiers = modifiers.FindModifiers("GSShotDelay");
                var reload = Math.Round(reloadModifiers.Aggregate(secondaryGun.Reload, (current, reloadModifier) => current * (decimal)reloadModifier), 2);

                var rangeModifiers = modifiers.FindModifiers("GSMaxDist");
                var range = Math.Round(rangeModifiers.Aggregate(secondary.MaxRange, (current, rangeModifier) => current * (decimal)rangeModifier), 2);

                var secondaryUI = new SecondaryBatteryUI
                {
                    Name = $"{secondaryGroup.Count} x {secondaryGun.NumBarrels} " + Localizer.Instance[secondaryGun.Name].Localization,
                    Range = Math.Round(range / 1000, 2),
                    Reload = reload,
                    RoF = Math.Round(60 / reload),
                };

                try
                {
                    secondaryUI.Shell = ShellUI.FromShellName(secondaryGun.AmmoList, modifiers, secondaryUI.RoF * secondaryGroup.Count * secondaryGun.NumBarrels).First();
                }
                catch (KeyNotFoundException e)
                {
                    // TODO: fix issue properly for next minor release
                    Logging.Logger.Warn(e, "One or more keys of the secondary data were not found.");
                    secondaryUI.Shell = new ShellUI
                    {
                        Name = "Error",
                        Type = "Error",
                        TheoreticalDPM = "-1",
                    };
                    secondaryUI.Shell.PropertyValueMapper = secondaryUI.Shell.ToPropertyMapping();
                }

                secondaryUI.SecondaryBatteryData = secondaryUI.ToPropertyMapping();

                result.Add(secondaryUI);
            }

            result.Last().IsLast = true;

            return result;
        }
    }
}
