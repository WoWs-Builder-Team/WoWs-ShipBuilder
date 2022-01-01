// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI.UnitTranslations;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ShellUI : IDataUi
    {
        public string Name { get; set; } = default!;

        [JsonIgnore]
        public string Index { get; set; } = default!;

        [DataUiUnit("KG")]
        public decimal Mass { get; set; }

        public string Type { get; set; } = default!;

        public decimal Damage { get; set; }

        public string TheoreticalDPM { get; set; } = default!;

        [DataUiUnit("M")]
        public decimal ExplosionRadius { get; set; }

        [JsonIgnore]
        public decimal SplashCoeff { get; set; }

        [DataUiUnit("MPS")]
        public decimal ShellVelocity { get; set; }

        public decimal AirDrag { get; set; }

        [DataUiUnit("MM")]
        public int Penetration { get; set; }

        [DataUiUnit("MM")]
        public decimal Overmatch { get; set; }

        [DataUiUnit("PerCent")]
        public decimal FireChance { get; set; }

        [DataUiUnit("PerCent")]
        public decimal FireChancePerSalvo { get; set; }

        [DataUiUnit("Degree")]
        public string? RicochetAngles { get; set; }

        [JsonIgnore]
        public decimal MinRicochetAngle { get; set; }

        [JsonIgnore]
        public decimal MaxRicochetAngle { get; set; }

        [DataUiUnit("MM")]
        public decimal ArmingThreshold { get; set; }

        [DataUiUnit("S")]
        public decimal FuseTimer { get; set; }

        [DataUiUnit("FPM")]
        public decimal PotentialFPM { get; set; }

        public decimal DepthExplosion { get; set; }

        [JsonIgnore]
        public bool IsLastEntry { get; private set; }

        [JsonIgnore]
        public bool ShowBlastPenetration { get; private set; }

        [JsonIgnore]
        public List<KeyValuePair<string, string>> PropertyValueMapper { get; set; } = default!;

        public static List<ShellUI> FromShellName(List<string> shellNames, List<(string Name, float Value)> modifiers, int barrelCount, decimal salvosPerMinute)
        {
            var shells = new List<ShellUI>();
            foreach (string shellName in shellNames)
            {
                var shell = AppDataHelper.Instance.GetProjectile<ArtilleryShell>(shellName);

                // Values that may be ignored depending on shell type
                var armingTreshold = Math.Round((decimal)shell.ArmingThreshold);
                var fuseTimer = Math.Round((decimal)shell.FuseTimer, 3);
                var overmatch = Math.Truncate((decimal)(shell.Caliber * 1000 / 14.3));

                float shellDamage = shell.Damage;
                float shellFireChance = shell.FireChance * 100;
                float shellPenetration = shell.Penetration;
                float shellAirDrag = shell.AirDrag;
                float shellMass = shell.Mass;
                bool showBlastPenetration = false;
                string shellType = "";

                switch (shell.ShellType)
                {
                    case ShellType.HE:
                    {
                        int index;
                        armingTreshold = 0;
                        fuseTimer = 0;
                        overmatch = 0;
                        showBlastPenetration = true;
                        shellType = UnitLocalization.ArmamentType_HE;

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
                        shellType = UnitLocalization.ArmamentType_SAP;
                        shellDamage = modifiers.FindModifiers("GMHECSDamageCoeff").Aggregate(shellDamage, (current, modifier) => current * modifier);
                        break;
                    }

                    case ShellType.AP:
                    {
                        // TODO: check and fix modifier names and application
                        int index;
                        shellType = UnitLocalization.ArmamentType_AP;
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

                var dpmNumber = Math.Round((decimal)shellDamage * barrelCount * salvosPerMinute);
                var fireChancePerSalvo = (decimal)(1 - Math.Pow((double)(1 - ((decimal)shellFireChance / 100)), barrelCount));

                NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = "'";

                var uiShell = new ShellUI
                {
                    Name = Localizer.Instance[shell.Name].Localization,
                    Type = shell.ShellType.ToString(),
                    Mass = (decimal)shellMass,
                    Damage = Math.Round((decimal)shellDamage),
                    ExplosionRadius = (decimal)shell.ExplosionRadius,
                    SplashCoeff = (decimal)shell.SplashCoeff,
                    ShellVelocity = Math.Round((decimal)shell.MuzzleVelocity, 1),
                    Penetration = (int)Math.Truncate(shellPenetration),
                    AirDrag = Math.Round((decimal)shellAirDrag, 2),
                    FireChance = Math.Round((decimal)shellFireChance, 1),
                    FireChancePerSalvo = Math.Round(fireChancePerSalvo * 100, 1),
                    PotentialFPM = Math.Round((decimal)shellFireChance / 100 * barrelCount * salvosPerMinute, 2),
                    Overmatch = overmatch,
                    ArmingThreshold = armingTreshold,
                    FuseTimer = fuseTimer,
                    TheoreticalDPM = dpmNumber.ToString("n0", nfi),
                    Index = shell.Name,
                    ShowBlastPenetration = showBlastPenetration,
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
