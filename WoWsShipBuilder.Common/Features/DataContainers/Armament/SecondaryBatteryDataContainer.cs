using System.Globalization;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial record SecondaryBatteryDataContainer : DataContainerBase
{
    public string Name { get; set; } = default!;

    public List<string> TurretName { get; set; } = new();

    public FormattedTextDataElement TurretSetup { get; set; } = default!;

    public decimal GunCaliber { get; set; }

    public int BarrelsCount { get; set; }

    public string BarrelsLayout { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal Range { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal Reload { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "ShotsPerMinute")]
    public decimal RoF { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public string TheoreticalDpm { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "FPM")]
    [DataElementFiltering(true, "ShouldDisplayFpm")]
    public decimal PotentialFpm { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal Sigma { get; set; }

    public ShellDataContainer? Shell { get; set; }

    public bool IsLast { get; set; }

    public bool DisplayFpm { get; set; }

    public static List<SecondaryBatteryDataContainer>? FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<(string, float)> modifiers)
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

            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = "'";

            int barrelCount = secondaryGroup.Count * secondaryGun.NumBarrels;

            var secondaryBatteryDataContainer = new SecondaryBatteryDataContainer
            {
                Name = arrangementString,
                TurretName = turretName,
                TurretSetup = new(arrangementString, turretName, ArgumentsTextKind: DataElementTextKind.LocalizationKey),
                BarrelsLayout = $"{secondaryGroup.Count} x {secondaryGun.NumBarrels}",
                BarrelsCount = secondaryGroup.Count * secondaryGun.NumBarrels,
                GunCaliber = Math.Round(secondaryGun.BarrelDiameter * 1000),
                Range = Math.Round(range / 1000, 2),
                Reload = Math.Round((decimal)reload, 2),
                RoF = Math.Round(rof * barrelCount, 1),
                Sigma = secondary.Sigma,
            };

            ShellDataContainer? shellData;
            try
            {
                shellData = ShellDataContainer.FromShellName(secondaryGun.AmmoList, modifiers, barrelCount, false).First();
            }
            catch (KeyNotFoundException e)
            {
                // TODO: fix issue properly for next minor release
                Logging.Logger.LogWarning(e, "One or more keys of the secondary data were not found");
                shellData = new()
                {
                    Name = "Error",
                    Type = "Error",
                    TheoreticalDpm = "-1",
                };
                secondaryBatteryDataContainer.UpdateDataElements();
            }

            secondaryBatteryDataContainer.Shell = shellData;
            secondaryBatteryDataContainer.DisplayFpm = shellData.Type.Equals($"ArmamentType_{ShellType.HE.ShellTypeToString()}");
            secondaryBatteryDataContainer.TheoreticalDpm = Math.Round(shellData.Damage * barrelCount * rof).ToString("n0", nfi);

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
        return this.DisplayFpm;
    }
}
