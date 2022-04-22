using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WoWsShipBuilder.DataStructures;

// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.Core.DataUI
{
    public record SpecialAbilityUI : DataContainerBase
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

        public static SpecialAbilityUI? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
        {
            SpecialAbilityUI specialUI;

            if (ship.SpecialAbility is not null)
            {
                var specialAbility = ship.SpecialAbility;

                specialUI = new()
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
                var artilleryConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Artillery);
                if (artilleryConfiguration == null)
                {
                    return null;
                }

                string[]? artilleryOptions = artilleryConfiguration.Components[ComponentType.Artillery];
                string[]? supportedModules = artilleryConfiguration.Components[ComponentType.Artillery];

                TurretModule? mainBattery;
                if (artilleryOptions.Length == 1)
                {
                    mainBattery = ship.MainBatteryModuleList[supportedModules.First()];
                }
                else
                {
                    string? hullArtilleryName = shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Artillery].First(artilleryName => supportedModules.Contains(artilleryName));
                    mainBattery = ship.MainBatteryModuleList[hullArtilleryName];
                }

                var burstMode = mainBattery.BurstModeAbility;

                if (burstMode is null)
                {
                    return null;
                }

                specialUI = new()
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
