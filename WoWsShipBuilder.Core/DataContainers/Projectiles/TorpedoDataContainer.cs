using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Projectile;

namespace WoWsShipBuilder.Core.DataContainers
{
    public partial record TorpedoDataContainer : ProjectileDataContainer
    {
        [DataElementType(DataElementTypes.KeyValue, IsValueLocalizationKey = true, IsValueAppLocalization = true)]
        public string TorpedoType { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValue, IsValueLocalizationKey = true)]
        [DataElementFiltering(true, "ShouldDisplayName")]
        public string Name { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValue)]
        public decimal Damage { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
        public decimal Range { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "Knots")]
        public decimal Speed { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
        public decimal Detectability { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
        public int ArmingDistance { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal ReactionTime { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
        public decimal FloodingChance { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = "Blast", UnitKey = "M")]
        public decimal ExplosionRadius { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = "Blast", TooltipKey = "BlastExplanation")]
        [DataElementFiltering(false)]
        public decimal SplashCoeff { get; set; }

        public List<ShipClass>? CanHitClasses { get; set; }

        public bool IsLast { get; set; }

        public bool IsFromPlane { get; set; }

        public static async Task<List<TorpedoDataContainer>> FromTorpedoName(List<string> torpedoNames, List<(string name, float value)> modifiers, bool fromPlane, IAppDataService appDataService)
        {
            var list = new List<TorpedoDataContainer>();
            foreach (string name in torpedoNames)
            {
                var torp = await appDataService.GetProjectile<Torpedo>(name);

                var torpedoDamageModifiers = modifiers.FindModifiers("torpedoDamageCoeff");
                var torpedoDamage = (decimal)torpedoDamageModifiers.Aggregate(torp.Damage, (current, modifier) => current * modifier);

                var torpedoSpeedModifiers = fromPlane ? modifiers.FindModifiers("planeTorpedoSpeedMultiplier").ToList() : modifiers.FindModifiers("torpedoSpeedMultiplier", true).ToList();

                var torpedoSpeed = (decimal)torpedoSpeedModifiers.Aggregate(torp.Speed, (current, modifier) => current * modifier);

                var torpedoDetectModifiers = modifiers.FindModifiers("torpedoVisibilityFactor");
                var torpedoDetect = (decimal)torpedoDetectModifiers.Aggregate(torp.SpottingRange, (current, modifier) => current * modifier);

                var torpedoArmingTimeModifiers = modifiers.FindModifiers("planeTorpedoArmingTimeCoeff");
                var torpedoArmingTime = (decimal)torpedoArmingTimeModifiers.Aggregate(torp.ArmingTime, (current, modifier) => current * modifier);

                var torpedoFloodingModifiers = fromPlane ? modifiers.FindModifiers("floodChanceFactorPlane", true) : modifiers.FindModifiers("floodChanceFactor", true);
                var torpedoFlooding = (decimal)torpedoFloodingModifiers.Aggregate(torp.FloodChance, (current, modifier) => current * modifier);

                var allClasses = new List<ShipClass> { ShipClass.Destroyer, ShipClass.Cruiser, ShipClass.Battleship, ShipClass.AirCarrier };

                // v = d/t --> d = v*t
                var torpedoDataContainer = new TorpedoDataContainer
                {
                    Name = name,
                    TorpedoType = $"ShipStats_Torpedo{torp.TorpedoType}",
                    Damage = Math.Round(torpedoDamage),
                    Range = Math.Round((decimal)torp.MaxRange / 1000, 1),
                    Speed = Math.Round(torpedoSpeed, 2),
                    Detectability = Math.Round(torpedoDetect, 2),
                    ArmingDistance = (int)Math.Round(torpedoSpeed * Constants.KnotsToMps * torpedoArmingTime, 0),
                    FloodingChance = Math.Round(torpedoFlooding * 100, 2),
                    ReactionTime = Math.Round(torpedoDetect / (torpedoSpeed * Constants.KnotsToMps) * 1000, 2),
                    ExplosionRadius = (decimal)torp.ExplosionRadius,
                    SplashCoeff = (decimal)torp.SplashCoeff,
                    IsFromPlane = fromPlane,
                };

                if (torp.IgnoreClasses != null && torp.IgnoreClasses.Any())
                {
                    torpedoDataContainer.CanHitClasses = allClasses.Except(torp.IgnoreClasses).ToList();
                }

                torpedoDataContainer.UpdateDataElements();
                list.Add(torpedoDataContainer);
            }

            return list;
        }

        private bool ShouldDisplayName(object obj)
        {
            return IsFromPlane;
        }
    }
}
