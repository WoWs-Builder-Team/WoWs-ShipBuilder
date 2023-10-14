using System.Globalization;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.DataContainers;

[DataContainer]
public partial record ConsumableDataContainer : DataContainerBase
{
    public string Name { get; set; } = default!;

    public string IconName { get; set; } = default!;

    public int Slot { get; set; }

    public string Desc { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    public string NumberOfUses { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal PreparationTime { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal Cooldown { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public decimal WorkTime { get; set; }

    public Dictionary<string, float> Modifiers { get; set; } = null!;

    public static ConsumableDataContainer FromTypeAndVariant(ShipConsumable consumable, List<(string name, float value)> modifiers, bool isCvPlanes, int shipHp, ShipClass shipClass)
    {
        return FromTypeAndVariant(consumable.ConsumableName, consumable.ConsumableVariantName, consumable.Slot, modifiers, isCvPlanes, shipHp, shipClass);
    }

    public static ConsumableDataContainer FromTypeAndVariant(AircraftConsumable consumable, List<(string name, float value)> modifiers, bool isCvPlanes, int shipHp, ShipClass shipClass)
    {
        return FromTypeAndVariant(consumable.ConsumableName, consumable.ConsumableVariantName, consumable.Slot, modifiers, isCvPlanes, shipHp, shipClass);
    }

    private static ConsumableDataContainer FromTypeAndVariant(string name, string variant, int slot, List<(string name, float value)> modifiers, bool isCvPlanes, int shipHp, ShipClass shipClass)
    {
        var consumableIdentifier = $"{name} {variant}";
        var usingFallback = false;
        if (!(AppData.ConsumableList?.TryGetValue(consumableIdentifier, out var consumable) ?? false))
        {
            Logging.Logger.LogError("Consumable {Identifier} not found in cached consumable list. Using dummy consumable instead", consumableIdentifier);
            usingFallback = true;
            consumable = new()
            {
                Index = "error",
                Name = "error",
                DescId = "error",
                Group = "error",
                IconId = "error",
                ConsumableVariantName = "error",
                Modifiers = new() { { "error", 1 } },
            };
        }

        var iconName = string.IsNullOrEmpty(consumable.IconId) ? name : consumable.IconId;
        var localizationKey = string.IsNullOrEmpty(consumable.DescId) ? consumable.Name : consumable.DescId;
        Dictionary<string, float> consumableModifiers = consumable.Modifiers is not null ? consumable.Modifiers.ToDictionary(x => x.Key, x => x.Value) : new();
        int uses = consumable.NumConsumables;
        float cooldown = consumable.ReloadTime;
        float workTime = consumable.WorkTime;
        float prepTime = consumable.PreparationTime;
        if (isCvPlanes && !consumableModifiers.ContainsKey("error"))
        {
            workTime = modifiers.FindModifiers("planeConsumablesWorkTime").Aggregate(workTime, (current, modifier) => current * modifier);

            if (name.Contains("PCY036", StringComparison.InvariantCultureIgnoreCase))
            {
                var workTimeModifiers = modifiers.FindModifiers("regenerateHealthWorkTimeCoeff");
                workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);

                var usesModifiers = modifiers.FindModifiers("regenerateHealthAdditionalConsumables");
                uses = usesModifiers.Aggregate(uses, (current, modifier) => (int)(current + modifier));

                var regenerationSpeedModifiers = modifiers.FindModifiers("planeRegenerationRate");
                var regenerationSpeed = regenerationSpeedModifiers.Aggregate(consumableModifiers["regenerationRate"], (current, modifier) => current * modifier);
                consumableModifiers["regenerationRate"] = regenerationSpeed;
            }
            else if (name.Contains("PCY035", StringComparison.InvariantCultureIgnoreCase))
            {
                var workTimeModifiers = modifiers.FindModifiers("callFightersWorkTimeCoeff");
                workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);

                var usesModifiers = modifiers.FindModifiers("callFightersAdditionalConsumables");
                uses = usesModifiers.Aggregate(uses, (current, modifier) => (int)(current + modifier));

                var radiusModifiers = modifiers.FindModifiers("callFightersRadiusCoeff");
                var radius = radiusModifiers.Aggregate(consumableModifiers["radius"], (current, modifier) => current * modifier);
                consumableModifiers["radius"] = radius;

                var timeDelayAttackModifiers = modifiers.FindModifiers("callFightersTimeDelayAttack");
                var timeDelayAttack = timeDelayAttackModifiers.Aggregate(consumableModifiers["timeDelayAttack"], (current, modifier) => current * modifier);
                consumableModifiers["timeDelayAttack"] = timeDelayAttack;

                var timeFromHeavenModifiers = modifiers.FindModifiers("callFightersAppearDelay");
                var timeFromHeaven = timeFromHeavenModifiers.Aggregate(consumableModifiers["timeFromHeaven"], (current, modifier) => current * modifier);
                consumableModifiers["timeFromHeaven"] = timeFromHeaven;

                var plane = AppData.FindAircraft(consumable.PlaneName.Substring(0, consumable.PlaneName.IndexOf("_", StringComparison.Ordinal)));
                consumableModifiers.Add("cruisingSpeed", plane.Speed);
                consumableModifiers.Add("concealment", (float)plane.ConcealmentFromShips);
                consumableModifiers.Add("maxKills", consumableModifiers["fightersNum"]);

                var maxViewDistanceModifiers = modifiers.FindModifiers("interceptorSelected").ToList();
                var baseMaxViewDistance = (float)plane.SpottingOnShips;
                var maxViewDistance = maxViewDistanceModifiers.Aggregate(baseMaxViewDistance, (current, modifier) => current * modifier);
                consumableModifiers["maxViewDistance"] = maxViewDistance;
                if (maxViewDistanceModifiers.Count > 0)
                {
                    iconName = $"{name}_Upgrade";
                    localizationKey = $"{consumable.Name}_Upgrade";
                }

                var planesConcealmentModifiers = modifiers.FindModifiers("planeVisibilityFactor");
                var planesConcealment = planesConcealmentModifiers.Aggregate(consumableModifiers["concealment"], (current, modifier) => current * modifier);
                consumableModifiers["concealment"] = planesConcealment;
            }
            else if (name.Contains("PCY034", StringComparison.InvariantCultureIgnoreCase))
            {
                var cooldownModifiers = modifiers.FindModifiers("healForsageReloadCoeff");
                cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);
            }
        }
        else if (!consumableModifiers.ContainsKey("error"))
        {
            var usesModifiers = modifiers.FindModifiers("additionalConsumables", true);
            uses = usesModifiers.Aggregate(uses, (current, modifier) => (int)(current + modifier));

            var talentUsesModifiers = modifiers.FindModifiers("numConsumables", true);
            uses = talentUsesModifiers.Aggregate(uses, (current, modifier) => (int)(current + modifier));

            var allCooldownModifiers = modifiers.FindModifiers($"ConsumableReloadTime_{shipClass.ShipClassToString()}", true);
            cooldown = allCooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);

            allCooldownModifiers = modifiers.FindModifiers("ConsumableReloadTime", true);
            cooldown = allCooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);

            var allWorkModifiers = modifiers.FindModifiers("ConsumablesWorkTime");
            workTime = allWorkModifiers.Aggregate(workTime, (current, modifier) => current * modifier);

            if (name.Contains("PCY011", StringComparison.InvariantCultureIgnoreCase))
            {
                var cooldownModifiers = modifiers.FindModifiers("airDefenseDispReloadCoeff");
                cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);

                var workTimeModifiers = modifiers.FindModifiers("airDefenseDispWorkTimeCoeff");
                workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
            }
            else if (name.Contains("PCY013", StringComparison.InvariantCultureIgnoreCase))
            {
                var extraScoutPlane = modifiers.FindModifiers("scoutAdditionalConsumables");
                uses = extraScoutPlane.Aggregate(uses, (current, modifier) => (int)(current + modifier));

                var cooldownModifiers = modifiers.FindModifiers("scoutReloadCoeff");
                cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);

                var workTimeModifiers = modifiers.FindModifiers("scoutWorkTimeCoeff");
                workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
            }
            else if (name.Contains("PCY010", StringComparison.InvariantCultureIgnoreCase))
            {
                var regenUsesModifiers = modifiers.FindModifiers("regenCrewAdditionalConsumables", true);
                uses = regenUsesModifiers.Aggregate(uses, (current, modifier) => (int)(current + modifier));

                var cooldownModifiers = modifiers.FindModifiers("regenCrewReloadCoeff");
                cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);

                var workTimeModifiers = modifiers.FindModifiers("regenCrewWorkTimeCoeff");
                workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);

                var regenerationSpeedModifiers = modifiers.FindModifiers("regenerationHPSpeed", true);
                var regenerationSpeed = regenerationSpeedModifiers.Aggregate(consumableModifiers["regenerationHPSpeed"], (current, modifier) => current * modifier);
                consumableModifiers["regenerationHPSpeed"] = regenerationSpeed;

                var hpPerHeal = (float)Math.Round(workTime * (regenerationSpeed * shipHp));
                consumableModifiers.Add("hpPerHeal", hpPerHeal);
            }
            else if (name.Contains("PCY016", StringComparison.InvariantCultureIgnoreCase))
            {
                var workTimeModifiers = modifiers.FindModifiers("sonarWorkTimeCoeff");
                workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);

                var torpDetectionModifiers = modifiers.FindModifiers("TorpedoDetectionCoefficient");
                var distTorpedo = torpDetectionModifiers.Aggregate(consumableModifiers["distTorpedo"], (current, modifier) => current * modifier);
                consumableModifiers["distTorpedo"] = distTorpedo;
            }
            else if (name.Contains("PCY020", StringComparison.InvariantCultureIgnoreCase))
            {
                var workTimeModifiers = modifiers.FindModifiers("rlsWorkTimeCoeff");
                workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
            }
            else if (name.Contains("PCY009", StringComparison.InvariantCultureIgnoreCase) || name.Contains("PCY037", StringComparison.InvariantCultureIgnoreCase))
            {
                var crashCrewUsesModifiers = modifiers.FindModifiers("crashCrewAdditionalConsumables", true);
                uses = crashCrewUsesModifiers.Aggregate(uses, (current, modifier) => (int)(current + modifier));

                var cooldownModifiers = modifiers.FindModifiers("crashCrewReloadCoeff");
                cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);

                var workTimeModifiers = modifiers.FindModifiers("crashCrewWorkTimeCoeff");
                workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
            }
            else if (name.Contains("PCY014", StringComparison.InvariantCultureIgnoreCase))
            {
                var workTimeModifiers = modifiers.FindModifiers("smokeGeneratorWorkTimeCoeff");
                workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);

                var smokeLifeTimeModifiers = modifiers.FindModifiers("smokeGeneratorLifeTime");
                var lifeTime = smokeLifeTimeModifiers.Aggregate(consumableModifiers["lifeTime"], (current, modifier) => current * modifier);
                consumableModifiers["lifeTime"] = lifeTime;

                var smokeUsesModifiers = modifiers.FindModifiers("smokeGeneratorAdditionalConsumables");
                uses = smokeUsesModifiers.Aggregate(uses, (current, modifier) => (int)(current + modifier));

                var smokeCooldownModifiers = modifiers.FindModifiers("smokeGeneratorReloadCoeff");
                cooldown = smokeCooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);
            }
            else if (name.Contains("PCY015", StringComparison.InvariantCultureIgnoreCase))
            {
                var workTimeModifiers = modifiers.FindModifiers("speedBoostersWorkTimeCoeff");
                workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
            }
            else if (name.Contains("PCY022", StringComparison.InvariantCultureIgnoreCase))
            {
                var cooldownModifiers = modifiers.FindModifiers("artilleryBoostersReloadCoeff");
                cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);
            }
            else if (name.Contains("PCY012", StringComparison.InvariantCultureIgnoreCase) || name.Contains("PCY038", StringComparison.InvariantCultureIgnoreCase))
            {
                var extraFighters = modifiers.FindModifiers("extraFighterCount");
                var totalFighters = extraFighters.Aggregate(consumableModifiers["fightersNum"], (current, modifier) => current + modifier);
                consumableModifiers["fightersNum"] = totalFighters;

                var cooldownModifiers = modifiers.FindModifiers("fighterReloadCoeff");
                cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);

                var plane = AppData.FindAircraft(consumable.PlaneName[..consumable.PlaneName.IndexOf("_", StringComparison.Ordinal)]);
                consumableModifiers.Add("cruisingSpeed", plane.Speed);
                consumableModifiers.Add("maxViewDistance", (float)plane.SpottingOnShips);
                consumableModifiers.Add("maxKills", consumableModifiers["fightersNum"]);

                var concealment = (float)plane.ConcealmentFromShips;
                var planesConcealmentModifiers = modifiers.FindModifiers("planeVisibilityFactor");
                var planesConcealment = planesConcealmentModifiers.Aggregate(concealment, (current, modifier) => current * modifier);
                consumableModifiers["concealment"] = planesConcealment;
            }
            else if (name.Contains("PCY018", StringComparison.InvariantCultureIgnoreCase))
            {
                var cooldownModifiers = modifiers.FindModifiers("torpedoReloaderReloadCoeff");
                cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);
            }
            else if (name.Contains("PCY045", StringComparison.InvariantCultureIgnoreCase))
            {
                var hydrophoneUpdateFrequencyModifiers = modifiers.FindModifiers("hydrophoneUpdateFrequencyCoeff");
                var hydrophoneUpdateFrequency = hydrophoneUpdateFrequencyModifiers.Aggregate(consumableModifiers["hydrophoneUpdateFrequency"], (current, modifier) => current * modifier);
                consumableModifiers["hydrophoneUpdateFrequency"] = hydrophoneUpdateFrequency;
            }
            else if (name.Contains("PCY048", StringComparison.InvariantCultureIgnoreCase))
            {
                var subsRadarPrepTimeModifiers = modifiers.FindModifiers("ConsumableReloadTime");
                prepTime = subsRadarPrepTimeModifiers.Aggregate(prepTime, (current, modifier) => current * modifier);
            }
        }
        else if (usingFallback)
        {
            Logging.Logger.LogWarning("Skipping consumable modifier calculation due to fallback consumable.");
        }

        var consumableDataContainer = new ConsumableDataContainer
        {
            Name = localizationKey,
            NumberOfUses = consumable.NumConsumables != -1 ? uses.ToString(CultureInfo.InvariantCulture) : "∞",
            IconName = iconName,
            Slot = slot,
            Desc = "",
            Cooldown = Math.Round((decimal)cooldown, 1),
            PreparationTime = Math.Round((decimal)prepTime, 1),
            WorkTime = Math.Round((decimal)workTime, 1),
            Modifiers = consumableModifiers,
        };

        consumableDataContainer.UpdateDataElements();
        return consumableDataContainer;
    }
}
