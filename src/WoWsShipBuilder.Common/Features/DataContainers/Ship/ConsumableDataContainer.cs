using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.DataContainers;

/// <summary>
/// Shared base for consumable data containers. Concrete rendering is provided by
/// <see cref="RegularConsumableDataContainer"/> (charge-based) and
/// <see cref="TimeBasedConsumableDataContainer"/> (time-based / "capacity"). The base resolves the
/// consumable, applies the build modifiers, then constructs the matching concrete container.
/// </summary>
public abstract class ConsumableDataContainer : DataContainerBase
{
    public string Name { get; set; } = default!;

    public string IconName { get; set; } = default!;

    public int Slot { get; set; }

    public ImmutableList<Modifier> Modifiers { get; set; } = ImmutableList<Modifier>.Empty;

    public static ConsumableDataContainer FromTypeAndVariant(ShipConsumable consumable, ImmutableList<Modifier> modifiers, bool isCvPlanes, int shipHp, ShipClass shipClass)
    {
        return FromTypeAndVariant(consumable.ConsumableName, consumable.ConsumableVariantName, consumable.Slot, modifiers, isCvPlanes, shipHp, shipClass);
    }

    public static ConsumableDataContainer FromTypeAndVariant(AircraftConsumable consumable, ImmutableList<Modifier> modifiers, bool isCvPlanes, int shipHp, ShipClass shipClass)
    {
        return FromTypeAndVariant(consumable.ConsumableName, consumable.ConsumableVariantName, consumable.Slot, modifiers, isCvPlanes, shipHp, shipClass);
    }

    private static ConsumableDataContainer FromTypeAndVariant(string name, string variant, int slot, ImmutableList<Modifier> modifiers, bool isCvPlanes, int shipHp, ShipClass shipClass)
    {
        var consumableIdentifier = $"{name} {variant}";
        var usingFallback = false;
        if (!AppData.ConsumableList.TryGetValue(consumableIdentifier, out var consumable))
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
                Modifiers = ImmutableList.Create(new Modifier("error", 1, "", null)),
            };
        }

        var iconName = string.IsNullOrEmpty(consumable.IconId) ? name : consumable.IconId;
        var localizationKey = string.IsNullOrEmpty(consumable.DescId) ? consumable.Name : consumable.DescId;

        var consumableState = new ConsumableState(name, consumable.NumConsumables, (decimal)consumable.ReloadTime, (decimal)consumable.WorkTime, iconName, localizationKey, consumable.Modifiers, (decimal)consumable.PreparationTime, (decimal)consumable.TimeBasedActiveTime);

        if (isCvPlanes && !consumable.Modifiers.Exists(x => x.Name.Equals("error", StringComparison.Ordinal)))
        {
            consumableState = ProcessAircraftConsumable(consumableState, modifiers, consumable);
        }
        else if (!consumable.Modifiers.Exists(x => x.Name.Equals("error", StringComparison.Ordinal)))
        {
            consumableState = ProcessShipConsumable(consumableState, modifiers, consumable, shipClass, shipHp);
        }
        else if (usingFallback)
        {
            Logging.Logger.LogWarning("Skipping consumable modifier calculation due to fallback consumable.");
        }

        return consumable.IsTimeBased
            ? TimeBasedConsumableDataContainer.Create(consumableState, slot)
            : RegularConsumableDataContainer.Create(consumableState, slot, consumable.NumConsumables);
    }

    private static ConsumableState ProcessAircraftConsumable(ConsumableState consumableState, ImmutableList<Modifier> modifiers, Consumable consumable)
    {
        var (name, uses, cooldown, workTime, iconName, localizationKey, consumableModifiersTmp, _, timeBasedActiveTime) = consumableState;
        var consumableModifiers = consumableModifiersTmp.ToList();

        uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.Plane", uses);
        cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.Plane", cooldown);
        workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.Plane", workTime);
        timeBasedActiveTime = modifiers.ApplyModifiers("ConsumableDataContainer.TimeBasedActiveTime.Plane", timeBasedActiveTime);

        uses = modifiers.ApplyModifiers($"ConsumableDataContainer.Uses.{consumable.Index}", uses);
        cooldown = modifiers.ApplyModifiers($"ConsumableDataContainer.Reload.{consumable.Index}", cooldown);
        workTime = modifiers.ApplyModifiers($"ConsumableDataContainer.WorkTime.{consumable.Index}", workTime);
        timeBasedActiveTime = modifiers.ApplyModifiers($"ConsumableDataContainer.TimeBasedActiveTime.{consumable.Index}", timeBasedActiveTime);

        if (name.Contains("PCY036", StringComparison.InvariantCultureIgnoreCase))
        {
            consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.PlaneRegenerationRate.PCY036", "regenerationRate");
        }
        else if (name.Contains("PCY035", StringComparison.InvariantCultureIgnoreCase))
        {
            consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.Radius.PCY035", "radius");
            consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.TimeDelayAttack.PCY035", "timeDelayAttack");
            consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.TimeDelayAppear.PCY035", "timeFromHeaven");

            var plane = AppData.FindAircraft(consumable.PlaneName[..consumable.PlaneName.IndexOf('_', StringComparison.Ordinal)]);
            var oldCruisingSpeed = consumableModifiers.Find(x => x.Name.Equals("cruisingSpeed", StringComparison.Ordinal));
            if (oldCruisingSpeed is not null)
            {
                consumableModifiers.Remove(oldCruisingSpeed);
            }

            consumableModifiers.Add(new("cruisingSpeed", plane.Speed, null, "ShipStats_CruisingSpeed", Unit.Knots, ImmutableHashSet<string>.Empty, DisplayValueProcessingKind.Raw, ValueProcessingKind.None));

            var oldConcealmentModifier = consumableModifiers.Find(x => x.Name.Equals("concealment", StringComparison.Ordinal));
            if (oldConcealmentModifier is not null)
            {
                consumableModifiers.Remove(oldConcealmentModifier);
            }

            consumableModifiers.Add(new("concealment", (float)plane.ConcealmentFromShips, null, "ShipStats_Concealment", Unit.Kilometers, ImmutableHashSet<string>.Empty, DisplayValueProcessingKind.Raw, ValueProcessingKind.None));

            var fightersNum = consumableModifiers.First(x => x.Name.Equals("fightersNum", StringComparison.Ordinal)).Value;
            var oldMaxKillModifier = consumableModifiers.Find(x => x.Name.Equals("maxKills", StringComparison.Ordinal));
            if (oldMaxKillModifier is not null)
            {
                consumableModifiers.Remove(oldMaxKillModifier);
            }

            consumableModifiers.Add(new("maxKills", fightersNum, null, "ModifierConverter_MaxKillsAmount", Unit.None, ImmutableHashSet<string>.Empty, DisplayValueProcessingKind.ToInt, ValueProcessingKind.None));

            var baseMaxViewDistance = (decimal)plane.SpottingOnShips;
            var maxViewDistance = (float)modifiers.ApplyModifiers("ConsumableDataContainer.Interceptor", baseMaxViewDistance);
            var maxViewDistanceModifier = consumableModifiers.Find(x => x.Name.Equals("maxViewDistance"));

            if (maxViewDistanceModifier is not null)
            {
                consumableModifiers.Remove(maxViewDistanceModifier);
            }

            consumableModifiers.Add(new("maxViewDistance", maxViewDistance, "", "ShipStats_MaxViewDistance", Unit.Kilometers, ImmutableHashSet<string>.Empty, DisplayValueProcessingKind.Raw, ValueProcessingKind.None));

            if (maxViewDistance == 0)
            {
                iconName = $"{name}_Upgrade";
                localizationKey = $"{consumable.Name}_Upgrade";
            }

            consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.Concealment.PCY035", "concealment");
        }

        return consumableState with { Uses = uses, Cooldown = cooldown, WorkTime = workTime, IconName = iconName, LocalizationKey = localizationKey, ConsumableModifiers = consumableModifiers.ToImmutableList(), TimeBasedActiveTime = timeBasedActiveTime };
    }

    private static ConsumableState ProcessShipConsumable(ConsumableState consumableState, ImmutableList<Modifier> modifiers, Consumable consumable, ShipClass shipClass, int shipHp)
    {
        var (name, uses, cooldown, workTime, iconName, localizationKey, consumableModifiersTmp, prepTime, timeBasedActiveTime) = consumableState;
        var consumableModifiers = consumableModifiersTmp.ToList();

        uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.Ship", uses);
        cooldown = modifiers.ApplyModifiers($"ConsumableDataContainer.Reload.{shipClass.ShipClassToString()}", cooldown);
        cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.Ship", cooldown);
        workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.Ship", workTime);
        timeBasedActiveTime = modifiers.ApplyModifiers("ConsumableDataContainer.TimeBasedActiveTime.Ship", timeBasedActiveTime);

        uses = modifiers.ApplyModifiers($"ConsumableDataContainer.Uses.{consumable.Index}", uses);
        cooldown = modifiers.ApplyModifiers($"ConsumableDataContainer.Reload.{consumable.Index}", cooldown);
        workTime = modifiers.ApplyModifiers($"ConsumableDataContainer.WorkTime.{consumable.Index}", workTime);
        timeBasedActiveTime = modifiers.ApplyModifiers($"ConsumableDataContainer.TimeBasedActiveTime.{consumable.Index}", timeBasedActiveTime);

        if (name.Contains("PCY010", StringComparison.InvariantCultureIgnoreCase))
        {
            // Repair party
            consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.RegenerationHpSpeed.PCY010", "consumable_regenerationHPSpeed");

            var regenSpeed = consumableModifiers.First(x => x.Name.Equals("consumable_regenerationHPSpeed", StringComparison.Ordinal)).Value;
            var hpPerHeal = (float)Math.Round(workTime * (decimal)(regenSpeed * shipHp));

            var oldModifier = consumableModifiers.Find(x => x.Name.Equals("hpPerHeal", StringComparison.Ordinal));
            if (oldModifier is not null)
            {
                consumableModifiers.Remove(oldModifier);
            }

            consumableModifiers.Add(new("hpPerHeal", hpPerHeal, null, "Consumable_HpPerHeal", Unit.None, ImmutableHashSet<string>.Empty, DisplayValueProcessingKind.Raw, ValueProcessingKind.None));
        }
        else if (name.Contains("PCY016", StringComparison.InvariantCultureIgnoreCase))
        {
            // Hydro
            consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.TorpDetection.PCY016", "distTorpedo");
        }
        else if (name.Contains("PCY014", StringComparison.InvariantCultureIgnoreCase))
        {
            // Smoke Generator
            consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.LifeTime.PCY014", "lifeTime");
        }
        else if (name.Contains("PCY012", StringComparison.InvariantCultureIgnoreCase) || name.Contains("PCY038", StringComparison.InvariantCultureIgnoreCase))
        {
            // Fighter, Fighter (auto)
            cooldown = modifiers.ApplyModifiers($"ConsumableDataContainer.Reload.{consumable.Index}", cooldown);

            consumableModifiers.UpdateConsumableModifierValue(modifiers, $"ConsumableDataContainer.ExtraFighters.{consumable.Index}", "fightersNum");
            var maxKills = consumableModifiers.First(x => x.Name.Equals("fightersNum", StringComparison.Ordinal)).Value;

            var plane = AppData.FindAircraft(consumable.PlaneName[..consumable.PlaneName.IndexOf('_', StringComparison.Ordinal)]);

            var oldCruisingModifier = consumableModifiers.Find(x => x.Name.Equals("cruisingSpeed", StringComparison.Ordinal));
            if (oldCruisingModifier is not null)
            {
                consumableModifiers.Remove(oldCruisingModifier);
            }

            consumableModifiers.Add(new("cruisingSpeed", plane.Speed, null, "ShipStats_CruisingSpeed", Unit.Knots, ImmutableHashSet<string>.Empty, DisplayValueProcessingKind.Raw, ValueProcessingKind.None));

            var oldMaxViewModifier = consumableModifiers.Find(x => x.Name.Equals("maxViewDistance", StringComparison.Ordinal));
            if (oldMaxViewModifier is not null)
            {
                consumableModifiers.Remove(oldMaxViewModifier);
            }

            consumableModifiers.Add(new("maxViewDistance", (float)plane.SpottingOnShips, null, "ShipStats_MaxViewDistance", Unit.Kilometers, ImmutableHashSet<string>.Empty, DisplayValueProcessingKind.Raw, ValueProcessingKind.None));

            var oldMaxKillsModifier = consumableModifiers.Find(x => x.Name.Equals("maxKills", StringComparison.Ordinal));
            if (oldMaxKillsModifier is not null)
            {
                consumableModifiers.Remove(oldMaxKillsModifier);
            }

            consumableModifiers.Add(new("maxKills", maxKills, null, "ModifierConverter_MaxKillsAmount", Unit.None, ImmutableHashSet<string>.Empty, DisplayValueProcessingKind.Raw, ValueProcessingKind.None));

            var concealment = (decimal)plane.ConcealmentFromShips;
            var planesConcealment = (float)modifiers.ApplyModifiers($"ConsumableDataContainer.Concealment.{consumable.Index}", concealment);
            var oldConcealmentModifier = consumableModifiers.Find(x => x.Name.Equals("concealment", StringComparison.Ordinal));
            if (oldConcealmentModifier is not null)
            {
                consumableModifiers.Remove(oldConcealmentModifier);
            }

            consumableModifiers.Add(new("concealment", planesConcealment, null, "ShipStats_Concealment", Unit.Kilometers, ImmutableHashSet<string>.Empty, DisplayValueProcessingKind.Raw, ValueProcessingKind.None));
        }
        else if (name.Contains("PCY045", StringComparison.InvariantCultureIgnoreCase))
        {
            // Hydrophone
            // used prior to 13.1
            consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.HydrophoneUpdateFrequency.PCY045", "hydrophoneUpdateFrequency");
        }
        else if (name.Contains("PCY048", StringComparison.InvariantCultureIgnoreCase))
        {
            // Submarine Surveillance
            prepTime = modifiers.ApplyModifiers("ConsumableDataContainer.PrepTime.PCY048", prepTime);
        }

        return consumableState with { Uses = uses, Cooldown = cooldown, WorkTime = workTime, IconName = iconName, LocalizationKey = localizationKey, ConsumableModifiers = consumableModifiers.ToImmutableList(), PrepTime = prepTime, TimeBasedActiveTime = timeBasedActiveTime };
    }

    internal readonly record struct ConsumableState(string Name, int Uses, decimal Cooldown, decimal WorkTime, string IconName, string LocalizationKey, ImmutableList<Modifier> ConsumableModifiers, decimal PrepTime, decimal TimeBasedActiveTime);
}
