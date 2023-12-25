using System.Globalization;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
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

    public decimal Range { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal Reload { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "ShotsPerMinute")]
    public decimal RoF { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public string TheoreticalDpm { get; set; } = default!;

    public int Dpm { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "FPM")]
    [DataElementFiltering(true, "ShouldDisplayFpm")]
    public decimal PotentialFpm { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal Sigma { get; set; }

    public ShellDataContainer? Shell { get; set; }

    public bool IsLast { get; set; }

    public bool DisplayFpm { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "DispersionAtMaxRange", UnitKey = "M")]
    public decimal HorizontalDisp { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "DispersionAtMaxRange", UnitKey = "M")]
    public decimal VerticalDisp { get; set; }

    public decimal DelimDist { get; set; }

    public decimal TaperDist { get; set; }

    public Dispersion DispersionData { get; set; } = default!;

    public double DispersionModifier { get; set; }

    public static List<SecondaryBatteryDataContainer>? FromShip(Ship ship, IEnumerable<ShipUpgrade> shipConfiguration, List<Modifier> modifiers)
    {
        var secondary = ship.Hulls[shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Hull][0]].SecondaryModule;
        if (secondary == null)
        {
            return null;
        }

        var result = new List<SecondaryBatteryDataContainer>();
        var groupedSecondaries = secondary.Guns.GroupBy(gun => new { gun.BarrelDiameter, gun.NumBarrels })
            .OrderBy(group => group.Key.BarrelDiameter)
            .ThenBy(group => group.Key.NumBarrels)
            .Select(group => group.ToList())
            .ToList();

        foreach (List<Gun> secondaryGroup in groupedSecondaries)
        {
            var secondaryGun = secondaryGroup[0];
            string arrangementString = $"{secondaryGroup.Count} x {secondaryGun.NumBarrels} {{0}}";
            List<string> turretName = new() { secondaryGun.Name };

            decimal reload = modifiers.ApplyModifiers("SecondaryBatteryDataContainer.Reload", secondaryGun.Reload);

            decimal range = modifiers.ApplyModifiers("SecondaryBatteryDataContainer.Range", secondary.MaxRange) / 1000;

            // Consider dispersion modifiers
            var dispersionModifier = (float)modifiers.ApplyModifiers("SecondaryBatteryDataContainer.Dispersion.IdealRadius", 1m);
            var dispersion = secondaryGun.Dispersion;
            var dispersionContainer = dispersion.CalculateDispersion((double)range * 1000, dispersionModifier);

            decimal rof = 60 / reload;

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
                Range = Math.Round(range, 2),
                Reload = Math.Round(reload, 2),
                RoF = Math.Round(rof * barrelCount, 1),
                Sigma = secondary.Sigma,
                DelimDist = (range * (decimal)dispersion.Delim) / 1000,
                TaperDist = (decimal)dispersion.TaperDist / 1000,
                HorizontalDisp = Math.Round((decimal)dispersionContainer.Horizontal, 2),
                VerticalDisp = Math.Round((decimal)dispersionContainer.Vertical, 2),
                DispersionData = dispersion,
                DispersionModifier = dispersionModifier,
            };

            ShellDataContainer? shellData;
            try
            {
                shellData = ShellDataContainer.FromShellName(secondaryGun.AmmoList, modifiers, barrelCount, false)[0];
            }
            catch (KeyNotFoundException e)
            {
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
            secondaryBatteryDataContainer.DisplayFpm = shellData.Type.Equals($"ArmamentType_{ShellType.HE.ShellTypeToString()}", StringComparison.Ordinal);
            secondaryBatteryDataContainer.Dpm = (int)Math.Round(shellData.Damage * barrelCount * rof);
            secondaryBatteryDataContainer.TheoreticalDpm = secondaryBatteryDataContainer.Dpm.ToString("n0", nfi);

            if (secondaryBatteryDataContainer.DisplayFpm)
            {
                secondaryBatteryDataContainer.PotentialFpm = Math.Round((shellData.ShellFireChance / 100) * barrelCount * rof, 2);
            }

            secondaryBatteryDataContainer.UpdateDataElements();

            result.Add(secondaryBatteryDataContainer);
        }

        result[^1].IsLast = true;

        return result;
    }

    private bool ShouldDisplayFpm(object obj)
    {
        return this.DisplayFpm;
    }
}
