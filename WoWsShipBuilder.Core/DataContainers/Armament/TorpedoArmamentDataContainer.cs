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

namespace WoWsShipBuilder.Core.DataContainers
{
    public partial record TorpedoArmamentDataContainer : DataContainerBase
    {
        [DataElementType(DataElementTypes.FormattedText, ValuesPropertyName = "LauncherNames", ArePropertyNameValuesKeys = true)]
        public string Name { get; set; } = default!;

        public List<string> LauncherNames { get; set; } = new();

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal TurnTime { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "DegreePerSecond")]
        public decimal TraverseSpeed { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal Reload { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Degree")]
        public string TorpedoArea { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal TimeToSwitch { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "FullSalvoDamage", NameLocalizationKey = "FirstOption")]
        public string TorpFullSalvoDmg { get; set; } = default!;

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "FullSalvoDamage", NameLocalizationKey = "SecondOption")]
        public string AltTorpFullSalvoDmg { get; set; } = default!;

        public int TorpCount { get; set; }

        public string TorpLayout { get; set; } = default!;

        public List<TorpedoDataContainer> Torpedoes { get; set; } = new();

        public IEnumerable<TorpedoLauncher> TorpedoLaunchers { get; private set; } = default!;

        public static async Task<TorpedoArmamentDataContainer?> FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers, IAppDataService appDataService)
        {
            var torpConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Torpedoes);
            if (torpConfiguration == null)
            {
                return null;
            }

            var torpedoModule = ship.TorpedoModules[torpConfiguration.Components[ComponentType.Torpedoes].First()];
            var launcher = torpedoModule.TorpedoLaunchers.First();

            List<(int BarrelCount, int LauncherCount, string LauncherName)> arrangementList = torpedoModule.TorpedoLaunchers
                .GroupBy(torpModule => torpModule.NumBarrels)
                .Select(group => (BarrelCount: group.Key, TorpCount: group.Count(), LauncherName: group.First().Name))
                .OrderBy(item => item.TorpCount)
                .ToList();

            var torpCount = 0;
            var arrangementString = "";
            var torpLayout = new string[arrangementList.Count];
            var launcherNames = new List<string>();

            for (var i = 0; i < arrangementList.Count; i++)
            {
                var current = arrangementList[i];
                launcherNames.Add(current.LauncherName);
                arrangementString += $"{current.LauncherCount}x{current.BarrelCount} {{{i}}}\n";
                torpLayout[i] = $"{current.LauncherCount}x{current.BarrelCount}";
                torpCount += current.LauncherCount * current.BarrelCount;
            }

            var turnSpeedModifiers = modifiers.FindModifiers("GTRotationSpeed");
            decimal traverseSpeed = turnSpeedModifiers.Aggregate(launcher.HorizontalRotationSpeed, (current, modifier) => current * (decimal)modifier);

            var reloadSpeedModifiers = modifiers.FindModifiers("GTShotDelay");
            decimal reloadSpeed = reloadSpeedModifiers.Aggregate(launcher.Reload, (current, modifier) => current * (decimal)modifier);

            var arModifiers = modifiers.FindModifiers("lastChanceReloadCoefficient");
            reloadSpeed = arModifiers.Aggregate(reloadSpeed, (current, arModifier) => current * (1 - ((decimal)arModifier / 100)));

            var talentModifiers = modifiers.FindModifiers("torpedoReloadCoeff");
            reloadSpeed = talentModifiers.Aggregate(reloadSpeed, (current, modifier) => current * (decimal)modifier);

            string torpedoArea = $"{launcher.TorpedoAngles[0]} - {Math.Round(launcher.TorpedoAngles[1], 1)}";

            var torpedoes = await TorpedoDataContainer.FromTorpedoName(launcher.AmmoList, modifiers, false, appDataService);

            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = "'";

            var torpedoArmamentDataContainer = new TorpedoArmamentDataContainer
            {
                Name = arrangementString,
                LauncherNames = launcherNames,
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = Math.Round(traverseSpeed, 2),
                Reload = Math.Round(reloadSpeed, 2),
                TorpedoArea = torpedoArea,
                Torpedoes = torpedoes,
                TimeToSwitch = Math.Round(torpedoModule.TimeToChangeAmmo,  1),
                TorpedoLaunchers = torpedoModule.TorpedoLaunchers,
                TorpLayout = string.Join(" + ", torpLayout),
                TorpCount = torpCount,
                TorpFullSalvoDmg = (torpCount * torpedoes.First().Damage).ToString("n0", nfi),
                AltTorpFullSalvoDmg = torpedoes.Count > 1 ? (torpCount * torpedoes.Last().Damage).ToString("n0", nfi) : default!,
            };

            torpedoArmamentDataContainer.Torpedoes.Last().IsLast = true;

            torpedoArmamentDataContainer.UpdateDataElements();
            return torpedoArmamentDataContainer;
        }
    }
}
