using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record TorpedoUI
    {
        [JsonIgnore]
        public string TorpedoName { get; set; } = default!;

        public decimal TorpedoRange { get; set; }

        public decimal TorpedoDamage { get; set; }

        public decimal TorpedoSpeed { get; set; }

        public decimal TorpedoDetectability { get; set; }

        public decimal TorpedoReactionTime { get; set; }

        public decimal TorpedoFloodingChance { get; set; }

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

                var torpedoSpeedModifiers = modifiers.FindModifiers("torpedoSpeedMultiplier");
                decimal torpedoSpeed = Math.Round((decimal)torpedoSpeedModifiers.Aggregate(torp.Speed, (current, modifier) => current * modifier), 2);

                var torpedoDetectModifiers = modifiers.FindModifiers("torpedoVisibilityFactor");
                decimal torpedoDetect = Math.Round((decimal)torpedoDetectModifiers.Aggregate(torp.SpottingRange, (current, modifier) => current * modifier), 2);

                var torpedoFloodingModifiers = modifiers.FindModifiers("floodChanceFactor");
                decimal torpedoFlooding = Math.Round((decimal)torpedoFloodingModifiers.Aggregate(torp.FloodChance, (current, modifier) => current * modifier), 2);

                var torpUI = new TorpedoUI
                {
                    TorpedoName = name,
                    TorpedoDamage = torpedoDamage,
                    TorpedoRange = (decimal)torp.MaxRange,
                    TorpedoSpeed = torpedoSpeed,
                    TorpedoDetectability = torpedoDetect,
                    TorpedoFloodingChance = torpedoFlooding,
                    TorpedoReactionTime = Math.Round(torpedoDetect / (torpedoSpeed * 0.0026m), 2),
                };
                list.Add(torpUI);
            }

            return list;
        }

    }
}
