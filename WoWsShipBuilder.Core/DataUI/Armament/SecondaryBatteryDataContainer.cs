using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public partial record SecondaryBatteryDataContainer : DataContainerBase
    {
        public string Name { get; set; } = default!;

        public List<string> TurretName { get; set; } = new();

        public FormattedTextDataElement TurretSetup { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
        public decimal Range { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal Reload { get; set; }

        [DataElementType(DataElementTypes.Tooltip, TooltipKey = "TrueReloadTooltip", UnitKey = "S")]
        public decimal TrueReload { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "ShotsPerMinute")]
        public decimal RoF { get; set; }

        [DataElementType(DataElementTypes.Tooltip, TooltipKey = "TrueReloadTooltip", UnitKey = "ShotsPerMinute")]
        public decimal TrueRoF { get; set; }

        [DataElementType(DataElementTypes.KeyValue)]
        public string TheoreticalDpm { get; set; } = default!;

        [DataElementType(DataElementTypes.Tooltip, TooltipKey = "TrueReloadTooltip")]
        public string TheoreticalTrueDpm { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "FPM")]
        [DataElementFiltering(true, "ShouldDisplayFpm")]
        public decimal PotentialFpm { get; set; }

        [DataElementType(DataElementTypes.KeyValue)]
        public decimal Sigma { get; set; }

        public ShellDataContainer? Shell { get; set; }

        public bool IsLast { get; set; }

        public bool DisplayFpm { get; set; }

        public static async Task<List<SecondaryBatteryDataContainer>?> FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers, IAppDataService appDataService)
        {
            var secondary = ship.Hulls[shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Hull].First()].SecondaryModule;
            if (secondary == null)
            {
                return null;
            }

            var result = new List<SecondaryBatteryDataContainer>();
            List<List<Gun>> groupedSecondaries = secondary.Guns.GroupBy(gun => new { gun.BarrelDiameter, gun.NumBarrels })
                .OrderBy(group => group.Key.BarrelDiameter)
                .ThenBy(group => group.Key.NumBarrels)
                .Select(group => group.ToList())
                .ToList();

            foreach (List<Gun> secondaryGroup in groupedSecondaries)
            {
                var secondaryGun = secondaryGroup.First();
                string arrangementString = $"{secondaryGroup.Count} x {secondaryGun.NumBarrels} {{0}}";
                List<string> turretName = new() { secondaryGun.Name };

                var reloadModifiers = modifiers.FindModifiers("GSShotDelay");
                float reload = reloadModifiers.Aggregate((float)secondaryGun.Reload, (current, reloadModifier) => current * reloadModifier);

                var arModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
                reload = arModifiers.Aggregate(reload, (current, arModifier) => current * (1 - (arModifier / 100)));

                var otherReloadModifiers = modifiers.FindModifiers("ATBAReloadCoeff");
                reload = otherReloadModifiers.Aggregate(reload, (current, modifier) => current * modifier);

                var rangeModifiers = modifiers.FindModifiers("GSMaxDist");
                decimal range = rangeModifiers.Aggregate(secondary.MaxRange, (current, rangeModifier) => current * (decimal)rangeModifier);

                decimal rof = 60 / (decimal)reload;

                decimal trueReload = Math.Ceiling((decimal)reload / Constants.TickRate) * Constants.TickRate;
                decimal trueRateOfFire = 60 / trueReload;

                var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = "'";

                int barrelCount = secondaryGroup.Count * secondaryGun.NumBarrels;

                var secondaryBatteryDataContainer = new SecondaryBatteryDataContainer
                {
                    Name = arrangementString,
                    TurretName = turretName,
                    TurretSetup = new(arrangementString, turretName, AreValuesKeys: true),
                    Range = Math.Round(range / 1000, 2),
                    Reload = Math.Round((decimal)reload, 2),
                    TrueReload = Math.Round(trueReload, 2),
                    TrueRoF = Math.Round(trueRateOfFire, 1),
                    RoF = Math.Round(rof, 1),
                    Sigma = secondary.Sigma,
                };

                ShellDataContainer? shellData;
                try
                {
                    shellData = (await ShellDataContainer.FromShellName(secondaryGun.AmmoList, modifiers, barrelCount, appDataService)).First();
                }
                catch (KeyNotFoundException e)
                {
                    // TODO: fix issue properly for next minor release
                    Logging.Logger.Warn(e, "One or more keys of the secondary data were not found.");
                    shellData = new()
                    {
                        Name = "Error",
                        Type = "Error",
                        TheoreticalDpm = "-1",
                    };
                    secondaryBatteryDataContainer.UpdateDataElements();
                }

                secondaryBatteryDataContainer.Shell = shellData;
                secondaryBatteryDataContainer.DisplayFpm = shellData.Type.Equals($"ArmamentType_{ShellType.HE}");
                secondaryBatteryDataContainer.TheoreticalDpm = Math.Round(shellData.Damage * barrelCount * rof).ToString("n0", nfi);
                secondaryBatteryDataContainer.TheoreticalTrueDpm = Math.Round(shellData.Damage * barrelCount * trueRateOfFire).ToString("n0", nfi);

                if (secondaryBatteryDataContainer.DisplayFpm)
                {
                    secondaryBatteryDataContainer.PotentialFpm = Math.Round(shellData.ShellFireChance / 100 * barrelCount * rof, 2);
                }

                secondaryBatteryDataContainer.UpdateDataElements();

                result.Add(secondaryBatteryDataContainer);
            }

            result.Last().IsLast = true;

            return result;
        }

        private bool ShouldDisplayFpm(object obj)
        {
            return DisplayFpm;
        }
    }
}
