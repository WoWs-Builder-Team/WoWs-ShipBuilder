// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Projectile;

namespace WoWsShipBuilder.Core.DataContainers;

public partial record ShellDataContainer : DataContainerBase
{
    public string Name { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue, IsValueLocalizationKey = true, IsValueAppLocalization = true)]
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

    public decimal Krupp { get; set; }

    public decimal MinRicochetAngle { get; set; }

    public decimal MaxRicochetAngle { get; set; }

    public bool IsLastEntry { get; private set; }

    public bool ShowBlastPenetration { get; private set; }

    public static List<ShellDataContainer> FromShellName(List<string> shellNames, List<(string Name, float Value)> modifiers, int barrelCount, bool isMainGunShell)
    {
        var shells = new List<ShellDataContainer>();
        foreach (string shellName in shellNames)
        {
            var shell = AppData.FindProjectile<ArtilleryShell>(shellName);

            // Values that may be ignored depending on shell type
            decimal armingThreshold = Math.Round((decimal)shell.ArmingThreshold);
            decimal fuseTimer = Math.Round((decimal)shell.FuseTimer, 3);
            decimal overmatch = Math.Truncate((decimal)(shell.Caliber * 1000 / 14.3));

            float shellDamage = shell.Damage;
            float shellFireChance = shell.FireChance * 100;
            float shellPenetration = shell.Penetration;
            float shellAirDrag = shell.AirDrag;
            float shellMass = shell.Mass;
            var showBlastPenetration = false;

            switch (shell.ShellType)
            {
                case ShellType.HE:
                {
                    int index;
                    overmatch = 0;
                    showBlastPenetration = true;

                    // IFHE fire chance malus
                    if (shell.Caliber > 0.139f)
                    {
                        index = modifiers.FindModifierIndex("burnChanceFactorHighLevel");
                        if (index.IsValidIndex())
                        {
                            shellFireChance *= modifiers[index].Value;
                        }
                    }
                    else
                    {
                        index = modifiers.FindModifierIndex("burnChanceFactorLowLevel");
                        if (index.IsValidIndex())
                        {
                            shellFireChance *= modifiers[index].Value;
                        }
                    }

                    // Victor Lima and India X-Ray signals
                    if (shell.Caliber > 0.160f)
                    {
                        shellFireChance += modifiers.FindModifiers("burnChanceFactorBig").Select(m => m * 100).Sum();
                    }
                    else
                    {
                        shellFireChance += modifiers.FindModifiers("burnChanceFactorSmall").Select(m => m * 100).Sum();
                    }

                    // Demolition expert
                    shellFireChance += modifiers.FindModifiers("artilleryBurnChanceBonus").Select(m => m * 100).Sum();

                    // Talent modifier
                    shellFireChance += modifiers.FindModifiers("burnProbabilityBonus").Select(m => m * 100).Sum();

                    // IFHE and possibly modifiers from supership abilities
                    var penModifierPrefix = isMainGunShell ? "GM" : "GS";
                    shellPenetration = modifiers.FindModifiers($"{penModifierPrefix}penetrationCoeffHE").Aggregate(shellPenetration, (current, modifier) => current * modifier);

                    goto case ShellType.SAP;
                }

                case ShellType.SAP:
                {
                    armingThreshold = 0;
                    fuseTimer = 0;
                    shellDamage = modifiers.FindModifiers("GMHECSDamageCoeff").Aggregate(shellDamage, (current, modifier) => current * modifier);
                    break;
                }

                case ShellType.AP:
                {
                    int index;
                    if (shell.Caliber >= 0.190f)
                    {
                        index = modifiers.FindModifierIndex("GMHeavyCruiserCaliberDamageCoeff");
                        if (index.IsValidIndex())
                        {
                            shellDamage *= modifiers[index].Value;
                        }
                    }

                    index = modifiers.FindModifierIndex("GMAPDamageCoeff");
                    if (index.IsValidIndex())
                    {
                        shellDamage *= modifiers[index].Value;
                    }

                    break;
                }
            }

            decimal minRicochet = Math.Round((decimal)shell.RicochetAngle, 1);
            decimal maxRicochet = Math.Round((decimal)shell.AlwaysRicochetAngle, 1);

            var fireChancePerSalvo = (decimal)(1 - Math.Pow((double)(1 - ((decimal)shellFireChance / 100)), barrelCount));

            var shellDataContainer = new ShellDataContainer
            {
                Name = shell.Name,
                Type = $"ArmamentType_{shell.ShellType.ShellTypeToString()}",
                Mass = (decimal)shellMass,
                Damage = Math.Round((decimal)shellDamage),
                ExplosionRadius = (decimal)shell.ExplosionRadius,
                SplashCoeff = (decimal)shell.SplashCoeff,
                ShellVelocity = Math.Round((decimal)shell.MuzzleVelocity, 2),
                Penetration = (int)Math.Truncate(shellPenetration),
                AirDrag = Math.Round((decimal)shellAirDrag, 2),
                ShellFireChance = Math.Round((decimal)shellFireChance, 1),
                FireChancePerSalvo = Math.Round(fireChancePerSalvo * 100, 1),
                Overmatch = overmatch,
                ArmingThreshold = armingThreshold,
                FuseTimer = fuseTimer,
                ShowBlastPenetration = showBlastPenetration,
                SplashRadius = (decimal)shell.DepthSplashRadius,
                SplashDmg = Math.Round((decimal)(shellDamage * shell.SplashDamageCoefficient)),
                Krupp = (decimal)shell.Krupp,
            };

            if (minRicochet > 0 || maxRicochet > 0)
            {
                shellDataContainer.MinRicochetAngle = minRicochet;
                shellDataContainer.MaxRicochetAngle = maxRicochet;
                shellDataContainer.RicochetAngles = $"{minRicochet} - {maxRicochet}";
            }

            shellDataContainer.UpdateDataElements();
            shells.Add(shellDataContainer);
        }

        shells.Last().IsLastEntry = true;
        return shells;
    }

    private bool ShouldDisplayBlastPenetration(object obj)
    {
        return ShowBlastPenetration;
    }
}
