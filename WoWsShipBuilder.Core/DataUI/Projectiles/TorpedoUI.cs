using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record TorpedoUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        public decimal Range { get; set; }

        public decimal Damage { get; set; }

        public decimal Speed { get; set; }

        public decimal Detectability { get; set; }

        public decimal ArmingDistance { get; set; }

        public decimal ReactionTime { get; set; }

        public decimal FloodingChance { get; set; }

        [JsonIgnore]
        public List<KeyValuePair<string, string>>? TorpedoData { get; set; }

        public static List<TorpedoUI> FromTorpedoName(List<string> torpedoNames, Nation nation, List<(string name, float value)> modifiers)
        {
            var list = new List<TorpedoUI>();
            foreach (var name in torpedoNames)
            {
                var torp = (Torpedo)AppData.ProjectileList![name];

                var torpedoDamageModifiers = modifiers.FindModifiers("torpedoDamageCoeff");
                decimal torpedoDamage = Math.Round((decimal)torpedoDamageModifiers.Aggregate(torp.Damage, (current, modifier) => current * modifier), 2);

                var torpedoSpeedModifiers = modifiers.FindModifiers("torpedoSpeedMultiplier").ToList();
                torpedoSpeedModifiers.AddRange(modifiers.FindModifiers("planeTorpedoSpeedMultiplier"));
                decimal torpedoSpeed = Math.Round((decimal)torpedoSpeedModifiers.Aggregate(torp.Speed, (current, modifier) => current * modifier), 2);

                var torpedoDetectModifiers = modifiers.FindModifiers("torpedoVisibilityFactor");
                decimal torpedoDetect = Math.Round((decimal)torpedoDetectModifiers.Aggregate(torp.SpottingRange, (current, modifier) => current * modifier), 2);

                var torpedoArmingTimeModifiers = modifiers.FindModifiers("planeTorpedoArmingTimeCoeff");
                decimal torpedoArmingTime = Math.Round((decimal)torpedoArmingTimeModifiers.Aggregate(torp.ArmingTime, (current, modifier) => current * modifier), 2);

                var torpedoFloodingModifiers = modifiers.FindModifiers("floodChanceFactor");
                decimal torpedoFlooding = Math.Round((decimal)torpedoFloodingModifiers.Aggregate(torp.FloodChance, (current, modifier) => current * modifier), 2);

                // v = d/t --> d = v*t
                var torpUI = new TorpedoUI
                {
                    Name = name,
                    Damage = torpedoDamage,
                    Range = (decimal)torp.MaxRange,
                    Speed = torpedoSpeed,
                    Detectability = torpedoDetect,
                    ArmingDistance = torpedoSpeed * 0.0026m * torpedoArmingTime,
                    FloodingChance = torpedoFlooding,
                    ReactionTime = Math.Round(torpedoDetect / (torpedoSpeed * 0.0026m), 2),
                };
                torpUI.TorpedoData = torpUI.ToPropertyMapping();
                list.Add(torpUI);
            }

            return list;
        }
    }
}
