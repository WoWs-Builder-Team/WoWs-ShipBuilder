using System.Collections.Immutable;
using System.Text.Json.Serialization;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable InconsistentNaming
namespace WoWsShipBuilder.Features.DataContainers;

[DataContainer]
public partial class SpecialAbilityDataContainer : DataContainerBase
{
    public string Name { get; set; } = default!;

    public string Description { get; set; } = default!;

    // These are for the special ability of ships like satsuma etc.
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal Duration { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal TargetAreaRadius { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public decimal ProgressPerAction { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal InactivityDelay { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ProgressLossInterval { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public decimal ProgressLossPerInterval { get; set; }

    // These are for Burst mode
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ReloadDuringBurst { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal ReloadAfterBurst { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public int ShotInBurst { get; set; }

    // This is in common
    [JsonIgnore]
    public ImmutableList<Modifier> Modifiers { get; set; } = ImmutableList<Modifier>.Empty;

    public bool IsBurstMode { get; set; }

    public static SpecialAbilityDataContainer? FromShip(Ship ship, ImmutableList<ShipUpgrade> shipConfiguration)
    {
        SpecialAbilityDataContainer specialDataContainer;

        if (ship.SpecialAbility is not null)
        {
            var specialAbility = ship.SpecialAbility;

            specialDataContainer = new()
            {
                Name = $"DOCK_RAGE_MODE_TITLE_{specialAbility.Name}",
                Description = specialAbility.ActivatorName.Equals("RibbonActivator") ? $"RAGE_MODE_TRIGGER_DESCRIPTION_{specialAbility.ActivatorName}" : $"RAGE_MODE_DESCRIPTION_{specialAbility.ActivatorName}",
                Duration = Math.Round((decimal)specialAbility.Duration, 1),
                TargetAreaRadius = Math.Round((decimal)(specialAbility.ActivatorRadius / 1000), 1),
                ProgressPerAction = (decimal)specialAbility.ProgressPerAction,
                InactivityDelay = (decimal)specialAbility.DecrementDelay,
                ProgressLossInterval = (decimal)specialAbility.DecrementPeriod,
                ProgressLossPerInterval = (decimal)specialAbility.DecrementCount,
                Modifiers = specialAbility.Modifiers,
            };

            specialDataContainer.UpdateDataElements();
        }
        else
        {
            var artilleryConfiguration = shipConfiguration.Find(c => c.UcType == ComponentType.Artillery);
            if (artilleryConfiguration == null)
            {
                return null;
            }

            ImmutableArray<string> artilleryOptions = artilleryConfiguration.Components[ComponentType.Artillery];
            ImmutableArray<string> supportedModules = artilleryConfiguration.Components[ComponentType.Artillery];

            TurretModule? mainBattery;
            if (artilleryOptions.Length == 1)
            {
                mainBattery = ship.MainBatteryModuleList[supportedModules[0]];
            }
            else
            {
                string hullArtilleryName = shipConfiguration.First(c => c.UcType == ComponentType.Hull).Components[ComponentType.Artillery].First(artilleryName => supportedModules.Contains(artilleryName));
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
