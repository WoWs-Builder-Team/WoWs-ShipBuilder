// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ShellUI : IDataUi
    {
        public string Name { get; set; } = default!;

        public string Type { get; set; } = default!;

        public decimal Damage { get; set; }

        [DataUiUnit("DPS")]
        public decimal TheoreticalDPM { get; set; }

        [DataUiUnit("MPS")]
        public decimal ShellVelocity { get; set; }

        [DataUiUnit("MM")]
        public decimal Penetration { get; set; }

        [DataUiUnit("PerCent")]
        public decimal FireChance { get; set; }

        [DataUiUnit("Degree")]
        public string? RicochetAngles { get; set; }

        [JsonIgnore]
        public decimal MinRicochetAngle { get; set; }

        [JsonIgnore]
        public decimal MaxRicochetAngle { get; set; }

        [DataUiUnit("MM")]
        public decimal Overmatch { get; set; }

        [DataUiUnit("MM")]
        public decimal ArmingThreshold { get; set; }

        [DataUiUnit("S")]
        public decimal FuseTimer { get; set; }

        public decimal DepthExplosion { get; set; }

        [JsonIgnore]
        public bool IsLastEntry { get; private set; }

        [JsonIgnore]
        public List<KeyValuePair<string, string>> PropertyValueMapper { get; set; } = default!;

        public static List<ShellUI> FromShellName(List<string> shellNames, List<(string Name, float Value)> modifiers, decimal dpmFactor)
        {
            //Gun gun = ship
            //    .MainBatteryModuleList[shipConfiguration.First(c => c.UcType == ComponentType.Artillery).Components[ComponentType.Artillery].First()]
            //    .Guns.First();
            var shells = new List<ShellUI>();
            foreach (string shellName in shellNames)
            {
                var shell = (ArtilleryShell)AppData.ProjectileList![shellName];

                // Values that may be ignored depending on shell type
                var armingTreshold = Math.Round((decimal)shell.ArmingThreshold);
                var fuseTimer = Math.Round((decimal)shell.FuseTimer, 3);

                float shellDamage = shell.Damage;
                float shellFireChance = shell.FireChance * 100;
                float shellPenetration = shell.Penetration;
                switch (shell.ShellType)
                {
                    case ShellType.HE:
                    {
                        int index;
                        armingTreshold = 0;
                        fuseTimer = 0;

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
                        var burnChanceModifierName = $"artilleryBurnChanceBonus";
                        shellFireChance += modifiers.FindModifiers(burnChanceModifierName).Select(m => m * 100).Sum();

                        // IFHE and possibly modifiers from supership abilities
                        shellPenetration = modifiers.FindModifiers("penetrationCoeffHE").Aggregate(shellPenetration, (current, modifier) => current * modifier);

                        goto case ShellType.SAP;
                    }

                    case ShellType.SAP:
                    {
                        armingTreshold = 0;
                        fuseTimer = 0;
                        shellDamage = modifiers.FindModifiers("GMHECSDamageCoeff").Aggregate(shellDamage, (current, modifier) => current * modifier);
                        break;
                    }

                    case ShellType.AP:
                    {
                        // TODO: check and fix modifier names and application
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

                var uiShell = new ShellUI
                {
                    Name = Localizer.Instance[shell.Name].Localization,
                    Type = shell.ShellType.ToString(),
                    Damage = Math.Round((decimal)shellDamage),
                    ShellVelocity = Math.Round((decimal)shell.MuzzleVelocity, 1),
                    Penetration = Math.Round((decimal)shellPenetration),
                    FireChance = Math.Round((decimal)shellFireChance, 1),
                    Overmatch = Math.Round((decimal)(shell.Caliber / 14.3)),
                    ArmingThreshold = armingTreshold,
                    FuseTimer = fuseTimer,
                    TheoreticalDPM = Math.Round((decimal)shellDamage * dpmFactor), // TODO: make into string with formatting
                };

                if (minRicochet > 0 || maxRicochet > 0)
                {
                    uiShell.MinRicochetAngle = minRicochet;
                    uiShell.MaxRicochetAngle = maxRicochet;
                    uiShell.RicochetAngles = $"{minRicochet} - {maxRicochet}";
                }

                uiShell.PropertyValueMapper = uiShell.ToPropertyMapping();
                shells.Add(uiShell);
            }

            shells.Last().IsLastEntry = true;
            return shells;
        }
    }
}
