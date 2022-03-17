using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SecondaryBatteryUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        [DataUiUnit("KM")]
        public decimal Range { get; set; }

        [JsonIgnore]
        public string TrueReload { get; set; } = default!;

        [DataUiUnit("ShotsPerMinute")]
        public decimal RoF { get; set; }

        [DataUiUnit("ShotsPerMinute")]
        public decimal TrueRoF { get; set; }

        public decimal Sigma { get; set; }

        [DataUiUnit("S")]
        public decimal Reload { get; set; }

        [JsonIgnore]
        public ShellUI? Shell { get; set; }

        [JsonIgnore]
        public bool IsLast { get; set; } = false;

        [JsonIgnore]
        public List<KeyValuePair<string, string>> SecondaryBatteryData { get; set; } = default!;

        public static List<SecondaryBatteryUI>? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers, IAppDataService appDataService)
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
                var reload = reloadModifiers.Aggregate((float)secondaryGun.Reload, (current, reloadModifier) => current * reloadModifier);

                var arModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
                reload = arModifiers.Aggregate(reload, (current, arModifier) => current * (1 - (arModifier / 100)));

                var otherReloadModifiers = modifiers.FindModifiers("ATBAReloadCoeff");
                reload = otherReloadModifiers.Aggregate(reload, (current, modifier) => current * modifier);

                var rangeModifiers = modifiers.FindModifiers("GSMaxDist");
                var range = Math.Round(rangeModifiers.Aggregate(secondary.MaxRange, (current, rangeModifier) => current * (decimal)rangeModifier), 2);

                var rof = Math.Round(60 / (decimal)reload, 1);

                var trueReload = Math.Ceiling((decimal)reload / Constants.TickRate) * Constants.TickRate;
                decimal trueRateOfFire = Math.Round(60 / trueReload, 1);

                var secondaryUI = new SecondaryBatteryUI
                {
                    Name = $"{secondaryGroup.Count} x {secondaryGun.NumBarrels} " + Localizer.Instance[secondaryGun.Name].Localization,
                    Range = Math.Round(range / 1000, 2),
                    Reload = Math.Round((decimal)reload, 2),
                    TrueReload = Math.Round(trueReload, 2) + " " + Translation.Unit_S,
                    TrueRoF = trueRateOfFire,
                    RoF = rof,
                    Sigma = secondary.Sigma,
                };

                try
                {
                    secondaryUI.Shell = ShellUI.FromShellName(secondaryGun.AmmoList, modifiers, secondaryGroup.Count * secondaryGun.NumBarrels, rof, trueRateOfFire, appDataService).First();
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
