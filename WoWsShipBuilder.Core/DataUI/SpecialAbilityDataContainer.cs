using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;

// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.Core.DataUI
{
    public partial record SpecialAbilityDataContainer : DataContainerBase
    {
        [DataElementType(DataElementTypes.Value)]
        public string Name { get; set; } = default!;

        [DataElementType(DataElementTypes.Value)]

        public string Description { get; set; } = default!;

        // These are for the special ability of ships like satsuma etc.
        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal Duration { get; set; }

        [DataElementType(DataElementTypes.KeyValue)]
        public int RequiredHits { get; set; }

        // These are for Burst mode
        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal ReloadDuringBurst { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
        public decimal ReloadAfterBurst { get; set; }

        [DataElementType(DataElementTypes.KeyValue)]
        public int ShotInBurst { get; set; }

        // This is in common
        [JsonIgnore]
        public Dictionary<string, float> Modifiers { get; set; } = null!;

        public bool IsBurstMode { get; set; } = false;

        public static SpecialAbilityDataContainer? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
        {
            SpecialAbilityDataContainer specialDataContainer;

            if (ship.SpecialAbility is not null)
            {
                var specialAbility = ship.SpecialAbility;

                specialDataContainer = new()
                {
                    Name = $"DOCK_RAGE_MODE_TITLE_{specialAbility.Name}",
                    Description = $"DOCK_RAGE_MODE_DESCRIPTION_{specialAbility.Name}",
                    Duration = Math.Round((decimal)specialAbility.Duration, 1),
                    RequiredHits = specialAbility.RequiredHits,
                    Modifiers = specialAbility.Modifiers,
                };

                specialDataContainer.UpdateDataElements();
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

                specialDataContainer = new()
                {
                    Name = "ShipStats_BurstMode",
                    ReloadDuringBurst = burstMode.ReloadDuringBurst,
                    ReloadAfterBurst = burstMode.ReloadAfterBurst,
                    ShotInBurst = burstMode.ShotInBurst,
                    Modifiers = burstMode.Modifiers,
                    IsBurstMode = true,
                };

                specialDataContainer.UpdateDataElements();
            }

            return specialDataContainer;
        }
    }
}
