using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SpecialAbilityUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        [JsonIgnore]
        public string Description { get; set; } = default!;

        // These are for the special ability of ships like satsuma etc.
        [DataUiUnit("S")]
        public decimal Duration { get; set; }

        public int RequiredHits { get; set; }

        // These are for Burst mode
        [DataUiUnit("S")]
        public decimal ReloadDuringBurst { get; set; }

        [DataUiUnit("S")]
        public decimal ReloadAfterBurst { get; set; }

        public int ShotInBurst { get; set; }

        // This is in common
        [JsonIgnore]
        public Dictionary<string, float> Modifiers { get; set; } = null!;

        [JsonIgnore]
        public bool IsBurstMode { get; set; } = false;

        [JsonIgnore]
        public List<KeyValuePair<string, string>> SpecialData { get; set; } = default!;

        public static SpecialAbilityUI? FromShip(Ship ship, List<(string name, float value)> modifiers)
        {
            if (ship.SpecialAbility is null && ship.BurstModeAbility is null)
            {
                return null;
            }

            SpecialAbilityUI specialUI;

            if (ship.SpecialAbility is not null)
            {
                var specialAbility = ship.SpecialAbility;

                specialUI = new SpecialAbilityUI
                {
                    Name = $"DOCK_RAGE_MODE_TITLE_{specialAbility.Name}",
                    Description = $"DOCK_RAGE_MODE_DESCRIPTION_{specialAbility.Name}",
                    Duration = Math.Round((decimal)specialAbility.Duration, 1),
                    RequiredHits = specialAbility.RequiredHits,
                    Modifiers = specialAbility.Modifiers,
                };

                specialUI.SpecialData = specialUI.ToPropertyMapping();
            }
            else
            {
                var burstMode = ship.BurstModeAbility;

                specialUI = new SpecialAbilityUI
                {
                    Name = "ShipStats_BurstMode",
                    ReloadDuringBurst = burstMode.ReloadDuringBurst,
                    ReloadAfterBurst = burstMode.ReloadAfterBurst,
                    ShotInBurst = burstMode.ShotInBurst,
                    Modifiers = burstMode.Modifiers,
                    IsBurstMode = true,
                };

                specialUI.SpecialData = specialUI.ToPropertyMapping();
            }

            return specialUI;
        }
    }
}
