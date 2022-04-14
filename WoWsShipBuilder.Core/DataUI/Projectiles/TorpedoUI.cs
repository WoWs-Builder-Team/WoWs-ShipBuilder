using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataUI.Projectiles;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record TorpedoUI : ProjectileUI, IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        public string Type { get; set; } = default!;

        [DataUiUnit("KM")]
        public decimal Range { get; set; }

        public decimal Damage { get; set; }

        [DataUiUnit("Knots")]
        public decimal Speed { get; set; }

        [DataUiUnit("KM")]
        public decimal Detectability { get; set; }

        [DataUiUnit("M")]
        public decimal ExplosionRadius { get; set; }

        [JsonIgnore]
        public decimal SplashCoeff { get; set; }

        [DataUiUnit("M")]
        public int ArmingDistance { get; set; }

        [DataUiUnit("S")]
        public decimal ReactionTime { get; set; }

        [DataUiUnit("PerCent")]
        public decimal FloodingChance { get; set; }

        [JsonIgnore]
        public List<ShipClass>? CanHitClasses { get; set; }

        [JsonIgnore]
        public bool IsLast { get; set; } = false;

        public static List<TorpedoUI> FromTorpedoName(List<string> torpedoNames, List<(string name, float value)> modifiers, bool fromPlane, IAppDataService appDataService)
        {
            var list = new List<TorpedoUI>();
            foreach (var name in torpedoNames)
            {
                var torp = appDataService.GetProjectile<Torpedo>(name);

                var torpedoDamageModifiers = modifiers.FindModifiers("torpedoDamageCoeff");
                decimal torpedoDamage = (decimal)torpedoDamageModifiers.Aggregate(torp.Damage, (current, modifier) => current * modifier);

                var torpedoSpeedModifiers = modifiers.FindModifiers("torpedoSpeedMultiplier").ToList();
                torpedoSpeedModifiers.AddRange(modifiers.FindModifiers("planeTorpedoSpeedMultiplier"));
                decimal torpedoSpeed = (decimal)torpedoSpeedModifiers.Aggregate(torp.Speed, (current, modifier) => current * modifier);

                var torpedoDetectModifiers = modifiers.FindModifiers("torpedoVisibilityFactor");
                decimal torpedoDetect = (decimal)torpedoDetectModifiers.Aggregate(torp.SpottingRange, (current, modifier) => current * modifier);

                var torpedoArmingTimeModifiers = modifiers.FindModifiers("planeTorpedoArmingTimeCoeff");
                decimal torpedoArmingTime = (decimal)torpedoArmingTimeModifiers.Aggregate(torp.ArmingTime, (current, modifier) => current * modifier);

                var torpedoFloodingModifiers = fromPlane ? modifiers.FindModifiers("floodChanceFactorPlane", true) : modifiers.FindModifiers("floodChanceFactor", true);
                decimal torpedoFlooding = (decimal)torpedoFloodingModifiers.Aggregate(torp.FloodChance, (current, modifier) => current * modifier);

                var allClasses = new List<ShipClass> { ShipClass.Destroyer, ShipClass.Cruiser, ShipClass.Battleship, ShipClass.AirCarrier };

                // v = d/t --> d = v*t
                var torpUI = new TorpedoUI
                {
                    Name = name,
                    Type = Translation.ShipStats_TorpedoStandard,
                    Damage = Math.Round(torpedoDamage),
                    Range = Math.Round((decimal)torp.MaxRange / 1000, 1),
                    Speed = Math.Round(torpedoSpeed, 2),
                    Detectability = Math.Round(torpedoDetect, 2),
                    ArmingDistance = (int)Math.Round(torpedoSpeed * Constants.KnotsToMps * torpedoArmingTime, 0),
                    FloodingChance = Math.Round(torpedoFlooding * 100, 2),
                    ReactionTime = Math.Round(torpedoDetect / (torpedoSpeed * Constants.KnotsToMps) * 1000, 2),
                    ExplosionRadius = (decimal)torp.ExplosionRadius,
                    SplashCoeff = (decimal)torp.SplashCoeff,
                };

                if (name.Contains("Magnetic", StringComparison.OrdinalIgnoreCase))
                {
                    torpUI.Type = Translation.ShipStats_TorpedoMagnetic;
                }

                if (torp.IgnoreClasses != null && torp.IgnoreClasses.Any())
                {
                    torpUI.CanHitClasses = allClasses.Except(torp.IgnoreClasses).ToList();
                    torpUI.Type = Translation.ShipStats_TorpedoDeepWater;
                }

                torpUI.ProjectileData = torpUI.ToPropertyMapping();
                list.Add(torpUI);
            }

            return list;
        }
    }
}
