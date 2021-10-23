// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ShellUI
    {
        public string Name { get; set; } = default!;

        public string Type { get; set; } = default!;

        public decimal Damage { get; set; }

        public decimal TheoreticalDPM { get; set; }

        public decimal Velocity { get; set; }

        public decimal Weight { get; set; }

        public decimal Penetration { get; set; }

        public decimal FireChance { get; set; }

        public (decimal, decimal) RicochetAngles { get; set; }

        public decimal Overmatch { get; set; }

        public decimal ArmingThreshold { get; set; }

        public decimal FuseTimer { get; set; }

        public decimal DepthExplosion { get; set; }

        public static List<ShellUI> FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string Name, float Value)> modifiers)
        {
            Gun gun = ship
                .MainBatteryModuleList[shipConfiguration.First(c => c.UcType == ComponentType.Artillery).Components[ComponentType.Artillery].First()]
                .Guns.First();
            var shells = new List<ShellUI>();
            var shellData = AppDataHelper.Instance.ReadLocalJsonData<ArtilleryShell>(ship.ShipNation, ServerType.Live)!;
            foreach (string shellName in gun.AmmoList)
            {
                var shell = shellData.First(shellEntry => shellEntry.Value.Name.Equals(shellName, StringComparison.InvariantCultureIgnoreCase)).Value;

                float shellDamage = shell.Damage;
                float shellFireChance = shell.FireChance;
                float shellPenetration = shell.Penetration;
                switch (shell.ShellType)
                {
                    case ShellType.HE:
                    {
                        int index;

                        // IFHE fire chance malus
                        if (gun.BarrelDiameter > 0.139M)
                        {
                            index = modifiers.FindModifierIndex("burnChanceFactorHighLevel");
                            if (index.IsValidIndex())
                            {
                                shellFireChance /= 2;
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
                        if (gun.BarrelDiameter > 0.160M)
                        {
                            shellFireChance += modifiers.FindModifiers("burnChanceFactorBig").Select(m => m * 100).Sum();
                        }
                        else
                        {
                            shellFireChance += modifiers.FindModifiers("burnChanceFactorSmall").Select(m => m * 100).Sum();
                        }

                        // Demolition expert
                        var burnChanceModifierName = $"artilleryBurnChanceBonus_{ship.ShipClass.ToString()}";
                        shellFireChance += modifiers.FindModifiers(burnChanceModifierName).Select(m => m * 100).Sum();

                        // IFHE and possibly modifiers from supership abilities
                        shellPenetration = modifiers.FindModifiers("penetrationCoeffHE").Aggregate(shellPenetration, (current, modifier) => current * modifier);

                        goto case ShellType.SAP;
                    }

                    case ShellType.SAP:
                    {
                        shellDamage = modifiers.FindModifiers("GMHECSDamageCoeff").Aggregate(shellDamage, (current, modifier) => current * modifier);
                        break;
                    }

                    case ShellType.AP:
                    {
                        int index = modifiers.FindModifierIndex("GMHeavyCruiserCaliberDamageCoeff");
                        if (index.IsValidIndex())
                        {
                            shellDamage *= modifiers[index].Value;
                        }

                        index = modifiers.FindModifierIndex("GMHeavyCruiserCaliberDamageCoeff");
                        if (index.IsValidIndex())
                        {
                            shellDamage *= modifiers[index].Value;
                        }

                        break;
                    }
                }

                var uiShell = new ShellUI
                {
                    Name = Localizer.Instance[shell.Name].Localization,
                    Type = shell.ShellType.ToString(),
                    Damage = Math.Round((decimal)shellDamage),
                    Velocity = Math.Round((decimal)shell.MuzzleVelocity, 1),
                    Weight = Math.Round((decimal)shell.Mass),
                    Penetration = Math.Round((decimal)shellPenetration),
                    FireChance = Math.Round((decimal)shellFireChance),
                    RicochetAngles = (Math.Round((decimal)shell.RicochetAngle, 1), Math.Round((decimal)shell.AlwaysRicochetAngle, 1)),
                    Overmatch = Math.Round((decimal)(shell.Caliber / 14.3)),
                    ArmingThreshold = Math.Round((decimal)shell.ArmingThreshold),
                    FuseTimer = Math.Round((decimal)shell.FuseTimer, 2),
                };
                shells.Add(uiShell);
            }

            return shells;
        }
    }
}
