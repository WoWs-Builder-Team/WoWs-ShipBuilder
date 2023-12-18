// ReSharper disable UnusedAutoPropertyAccessor.Global

using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial record ShellDataContainer : DataContainerBase
{
    public string Name { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue, ValueTextKind = TextKind.AppLocalizationKey)]
    public string Type { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal Damage { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KG")]
    public decimal Mass { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Splash", TooltipKey = "SplashExplanation", UnitKey = "M")]
    public decimal SplashRadius { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Splash", TooltipKey = "SplashExplanation")]
    public decimal SplashDmg { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public string TheoreticalDpm { get; set; } = default!;

    [DataElementType(DataElementTypes.Tooltip, TooltipKey = "TrueReloadTooltip")]
    public string TheoreticalTrueDpm { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MPS")]
    public decimal ShellVelocity { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal AirDrag { get; set; }

    [DataElementType(DataElementTypes.Tooltip, TooltipKey = "KruppExplanation")]
    public decimal Krupp { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MM")]
    public int Penetration { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MM")]
    public decimal Overmatch { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "FireChance", UnitKey = "PerCent")]
    public decimal ShellFireChance { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "FireChance", UnitKey = "PerCent")]
    public decimal FireChancePerSalvo { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Degree")]
    public string? RicochetAngles { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MM")]
    public decimal ArmingThreshold { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal FuseTimer { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Blast", UnitKey = "M")]
    public decimal ExplosionRadius { get; set; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Blast", TooltipKey = "BlastExplanation")]
    [DataElementFiltering(true, "ShouldDisplayBlastPenetration")]
    public decimal SplashCoeff { get; set; }

    public decimal MinRicochetAngle { get; set; }

    public decimal MaxRicochetAngle { get; set; }

    public bool IsLastEntry { get; private set; }

    public bool ShowBlastPenetration { get; private set; }

    public static List<ShellDataContainer> FromShellName(IEnumerable<string> shellNames, List<Modifier> modifiers, int barrelCount, bool isMainGunShell)
    {
        var shells = shellNames.Select(shellName => ProcessShell(modifiers, barrelCount, isMainGunShell, shellName)).ToList();
        shells[^1].IsLastEntry = true;
        return shells;
    }

    private static ShellDataContainer ProcessShell(List<Modifier> modifiers, int barrelCount, bool isMainGunShell, string shellName)
    {
        var shell = AppData.FindProjectile<ArtilleryShell>(shellName);

        // Values that may be ignored depending on shell type
        decimal armingThreshold = Math.Round((decimal)shell.ArmingThreshold);
        decimal fuseTimer = Math.Round((decimal)shell.FuseTimer, 3);
        decimal overmatch = Math.Truncate((decimal)(shell.Caliber * 1000 / 14.3));

        decimal shellDamage = (decimal)shell.Damage;
        decimal shellFireChance = (decimal)shell.FireChance * 100;
        decimal shellPenetration = (decimal)shell.Penetration;
        float shellAirDrag = shell.AirDrag;
        float shellMass = shell.Mass;
        var showBlastPenetration = false;

        switch (shell.ShellType)
        {
            case ShellType.HE:
            {
                overmatch = 0;
                showBlastPenetration = true;

                // IFHE fire chance malus
                shellFireChance = modifiers.ApplyModifiers(shell.Caliber > 0.139f ? "ShellDataContainer.FireChance.Big.Multiplier" : "ShellDataContainer.FireChance.Small.Multiplier", shellFireChance);

                // Victor Lima and India X-Ray signals
                shellFireChance = modifiers.ApplyModifiers(shell.Caliber > 0.160f ? "ShellDataContainer.FireChance.Big.Additive" : "ShellDataContainer.FireChance.Small.Additive", shellFireChance);

                // Demolition expert and talent
                shellFireChance = modifiers.ApplyModifiers("ShellDataContainer.FireChance", shellFireChance);

                // IFHE and possibly modifiers from supership abilities
                var gunType = isMainGunShell ? "Primary" : "Secondary";
                shellPenetration = modifiers.ApplyModifiers($"ShellDataContainer.Penetration.{gunType}", shellPenetration);
                goto case ShellType.SAP;
            }

            case ShellType.SAP:
            {
                armingThreshold = 0;
                fuseTimer = 0;
                shellDamage = modifiers.ApplyModifiers("ShellDataContainer.Damage.HESAP", shellDamage);
                break;
            }

            case ShellType.AP:
            {
                if (shell.Caliber >= 0.190f)
                {
                    shellDamage = modifiers.ApplyModifiers("ShellDataContainer.Damage.BigAp", shellDamage);
                }

                shellDamage = modifiers.ApplyModifiers("ShellDataContainer.Damage.Ap", shellDamage);
                break;
            }
        }

        decimal minRicochet = Math.Round((decimal)shell.RicochetAngle, 1);
        decimal maxRicochet = Math.Round((decimal)shell.AlwaysRicochetAngle, 1);

        var fireChancePerSalvo = (decimal)(1 - Math.Pow((double)(1 - ((decimal)shellFireChance / 100)), barrelCount));

        var splashRadius = modifiers.ApplyModifiers("ShellDataContainer.UnderwaterSplash", (decimal)shell.DepthSplashRadius);

        var shellDataContainer = new ShellDataContainer
        {
            Name = shell.Name,
            Type = $"ArmamentType_{shell.ShellType.ShellTypeToString()}",
            Mass = (decimal)shellMass,
            Damage = Math.Round(shellDamage),
            ExplosionRadius = (decimal)shell.ExplosionRadius,
            SplashCoeff = (decimal)shell.SplashCoeff,
            ShellVelocity = Math.Round((decimal)shell.MuzzleVelocity, 2),
            Penetration = (int)Math.Truncate(shellPenetration),
            AirDrag = Math.Round((decimal)shellAirDrag, 2),
            ShellFireChance = Math.Round(shellFireChance, 1),
            FireChancePerSalvo = Math.Round(fireChancePerSalvo * 100, 1),
            Overmatch = overmatch,
            ArmingThreshold = armingThreshold,
            FuseTimer = fuseTimer,
            ShowBlastPenetration = showBlastPenetration,
            SplashRadius = Math.Round(splashRadius, 1),
            SplashDmg = Math.Round(shellDamage * (decimal)shell.SplashDamageCoefficient),
            Krupp = (decimal)shell.Krupp,
        };

        if (minRicochet > 0 || maxRicochet > 0)
        {
            shellDataContainer.MinRicochetAngle = minRicochet;
            shellDataContainer.MaxRicochetAngle = maxRicochet;
            shellDataContainer.RicochetAngles = $"{minRicochet} - {maxRicochet}";
        }

        shellDataContainer.UpdateDataElements();
        return shellDataContainer;
    }

    private bool ShouldDisplayBlastPenetration(object obj)
    {
        return this.ShowBlastPenetration;
    }
}
