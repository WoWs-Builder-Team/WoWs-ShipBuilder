using System.Globalization;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.DataContainers;

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

    public List<Modifier> Modifiers { get; set; } = null!;

    public static ConsumableDataContainer FromTypeAndVariant(ShipConsumable consumable, List<Modifier> modifiers, bool isCvPlanes, int shipHp, ShipClass shipClass)
    {
        return FromTypeAndVariant(consumable.ConsumableName, consumable.ConsumableVariantName, consumable.Slot, modifiers, isCvPlanes, shipHp, shipClass);
    }

    public static ConsumableDataContainer FromTypeAndVariant(AircraftConsumable consumable, List<Modifier> modifiers, bool isCvPlanes, int shipHp, ShipClass shipClass)
    {
        return FromTypeAndVariant(consumable.ConsumableName, consumable.ConsumableVariantName, consumable.Slot, modifiers, isCvPlanes, shipHp, shipClass);
    }

    private static ConsumableDataContainer FromTypeAndVariant(string name, string variant, int slot, List<Modifier> modifiers, bool isCvPlanes, int shipHp, ShipClass shipClass)
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
                Modifiers = new() { new Modifier("error", 1, "", null) },
            };

        }

        var iconName = string.IsNullOrEmpty(consumable.IconId) ? name : consumable.IconId;
        var localizationKey = string.IsNullOrEmpty(consumable.DescId) ? consumable.Name : consumable.DescId;
        var consumableModifiers = consumable.Modifiers.ToList();
        int uses = consumable.NumConsumables;
        decimal cooldown = (decimal)consumable.ReloadTime;
        decimal workTime = (decimal)consumable.WorkTime;
        decimal prepTime = (decimal)consumable.PreparationTime;
        if (isCvPlanes && !consumableModifiers.Any(x => x.Name.Equals("error", StringComparison.Ordinal)))
        {
            uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.Planes", uses);

            workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.Plane", workTime);

            if (name.Contains("PCY036", StringComparison.InvariantCultureIgnoreCase))
            {
                workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.PCY036", workTime);

                uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.PCY036", uses);

                consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.PlaneRegenerationRate.PCY036", "regenerationRate");
            }
            else if (name.Contains("PCY035", StringComparison.InvariantCultureIgnoreCase))
            {
                workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.PCY035", workTime);

                uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.PCY035", uses);

                consumableModifiers.UpdateConsumableModifierValue(modifiers,"ConsumableDataContainer.Radius.PCY035", "radius");
                consumableModifiers.UpdateConsumableModifierValue(modifiers,"ConsumableDataContainer.TimeDelayAttack.PCY035", "timeDelayAttack");
                consumableModifiers.UpdateConsumableModifierValue(modifiers,"ConsumableDataContainer.TimeDelayAppear.PCY035", "timeFromHeaven");

                //TODO check display value kind and translation
                var plane = AppData.FindAircraft(consumable.PlaneName.Substring(0, consumable.PlaneName.IndexOf("_", StringComparison.Ordinal)));
                var oldCruisingSpeed = consumableModifiers.Find(x => x.Name.Equals("cruisingSpeed", StringComparison.Ordinal));
                if (oldCruisingSpeed is not null)
                {
                    consumableModifiers.Remove(oldCruisingSpeed);
                }

                consumableModifiers.Add(new("", "cruisingSpeed", plane.Speed, null, "ShipStats_CruisingSpeed", Unit.Knots, new(), DisplayValueProcessingKind.None, ValueProcessingKind.None ));

                var oldConcealmentModifier = consumableModifiers.Find(x => x.Name.Equals("concealment", StringComparison.Ordinal));
                if (oldConcealmentModifier is not null)
                {
                    consumableModifiers.Remove(oldConcealmentModifier);
                }

                consumableModifiers.Add(new("", "concealment", (float)plane.ConcealmentFromShips, null, "ShipStats_Concealment", Unit.KM, new(), DisplayValueProcessingKind.None, ValueProcessingKind.None ));

                var fightersNum = consumableModifiers.First(x => x.Name.Equals("fightersNum", StringComparison.Ordinal)).Value;
                var oldMaxKillModifier = consumableModifiers.Find(x => x.Name.Equals("maxKills", StringComparison.Ordinal));
                if (oldMaxKillModifier is not null)
                {
                    consumableModifiers.Remove(oldMaxKillModifier);
                }

                consumableModifiers.Add(new("", "maxKills", fightersNum, null, "ModifierConverter_MaxKillsAmount", Unit.None, new(), DisplayValueProcessingKind.ToInt, ValueProcessingKind.None));

                var baseMaxViewDistance = (decimal)plane.SpottingOnShips;
                var maxViewDistance = (float)modifiers.ApplyModifiers("ConsumableDataContainer.Interceptor", baseMaxViewDistance);
                var maxViewDistanceModifier = consumableModifiers.Find(x => x.Name.Equals("maxViewDistance"));

                if (maxViewDistanceModifier is not null)
                {
                    consumableModifiers.Remove(maxViewDistanceModifier);
                }

                consumableModifiers.Add(new ("", "maxViewDistance", maxViewDistance, "", "ShipStats_MaxViewDistance", Unit.KM, new(), DisplayValueProcessingKind.None, ValueProcessingKind.None));

                if (maxViewDistance == 0)
                {
                    iconName = $"{name}_Upgrade";
                    localizationKey = $"{consumable.Name}_Upgrade";
                }

                consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.PCY035.Concealment", "concealment");
            }
            else if (name.Contains("PCY034", StringComparison.InvariantCultureIgnoreCase))
            {
                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.PCY035.Reload", cooldown);
            }
        }
        else if (!consumableModifiers.Any(x => x.Name.Equals("error", StringComparison.Ordinal)))
        {
            uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.Ship", uses);

            cooldown = modifiers.ApplyModifiers($"ConsumableDataContainer.Reload.{shipClass.ShipClassToString()}", cooldown);
            cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.Ship", cooldown);

            workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.Ship", workTime);

            if (name.Contains("PCY011", StringComparison.InvariantCultureIgnoreCase))
            {
                // Defensive AA
                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY011", cooldown);

                workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.PCY011", workTime);
            }
            else if (name.Contains("PCY013", StringComparison.InvariantCultureIgnoreCase))
            {
                // Spotting Aircraft
                uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.PCY013", uses);

                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY013", cooldown);

                workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.PCY013", workTime);
            }
            else if (name.Contains("PCY010", StringComparison.InvariantCultureIgnoreCase))
            {
                // Repair party
                uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.PCY010", uses);

                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY010", cooldown);

                workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.PCY010", workTime);

                consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.RegenerationHpSpeed.PCY010", "consumable_regenerationHPSpeed");

                var regenSpeed = consumableModifiers.First(x => x.Name.Equals("consumable_regenerationHPSpeed", StringComparison.Ordinal))!.Value;
                var hpPerHeal = (float)Math.Round(workTime * (decimal)(regenSpeed * shipHp));

                var oldModifier = consumableModifiers.Find(x => x.Name.Equals("hpPerHeal"));
                if (oldModifier is not null)
                {
                    consumableModifiers.Remove(oldModifier);
                }

                consumableModifiers.Add(new("", "hpPerHeal", hpPerHeal, null, "Consumable_HpPerHeal", Unit.None, new(), DisplayValueProcessingKind.None, ValueProcessingKind.None));
            }
            else if (name.Contains("PCY016", StringComparison.InvariantCultureIgnoreCase))
            {
                // Hydro
                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY016", cooldown);

                workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.PCY016", workTime);

                consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.TorpDetection.PCY016", "distTorpedo");
            }
            else if (name.Contains("PCY020", StringComparison.InvariantCultureIgnoreCase))
            {
                // Radar
                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY020", cooldown);

                workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.PCY020", workTime);
            }
            else if (name.Contains("PCY009", StringComparison.InvariantCultureIgnoreCase) || name.Contains("PCY037", StringComparison.InvariantCultureIgnoreCase))
            {
                // Damage Control Party, Damage Control Party (auto)
                uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.PCY009.PCY037", uses);

                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY009.PCY037", cooldown);

                workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.PCY009.PCY037", workTime);
            }
            else if (name.Contains("PCY014", StringComparison.InvariantCultureIgnoreCase))
            {
                // Smoke Generator
                uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.PCY014", uses);

                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY014", cooldown);

                workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.PCY014", workTime);

                consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.LifeTime.PCY014", "lifeTime");
            }
            else if (name.Contains("PCY015", StringComparison.InvariantCultureIgnoreCase))
            {
                // Engine Boost
                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY015", cooldown);

                workTime = modifiers.ApplyModifiers("ConsumableDataContainer.WorkTime.PCY015", workTime);
            }
            else if (name.Contains("PCY022", StringComparison.InvariantCultureIgnoreCase))
            {
                // Main Battery Reload Booster
                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY022", cooldown);
            }
            else if (name.Contains("PCY012", StringComparison.InvariantCultureIgnoreCase) || name.Contains("PCY038", StringComparison.InvariantCultureIgnoreCase))
            {
                // Fighter, Fighter (auto)
                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY012.PCY038", cooldown);

                consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.ExtraFighters.PCY012.PCY03", "fightersNum");
                var maxKills = consumableModifiers.First(x => x.Name.Equals("fightersNum", StringComparison.Ordinal))!.Value;

                var plane = AppData.FindAircraft(consumable.PlaneName[..consumable.PlaneName.IndexOf("_", StringComparison.Ordinal)]);

                var oldCruisingModifier = consumableModifiers.Find(x => x.Name.Equals("cruisingSpeed"));
                if (oldCruisingModifier is not null)
                {
                    consumableModifiers.Remove(oldCruisingModifier);
                }

                consumableModifiers.Add(new("", "cruisingSpeed", plane.Speed, null, "ShipStats_CruisingSpeed", Unit.Knots, new(), DisplayValueProcessingKind.None, ValueProcessingKind.None));

                var oldMaxViewModifier = consumableModifiers.Find(x => x.Name.Equals("maxViewDistance"));
                if (oldMaxViewModifier is not null)
                {
                    consumableModifiers.Remove(oldMaxViewModifier);
                }

                consumableModifiers.Add(new("", "maxViewDistance", (float)plane.SpottingOnShips, null, "ShipStats_MaxViewDistance", Unit.KM, new(), DisplayValueProcessingKind.None, ValueProcessingKind.None));

                var oldMaxKillsModifier = consumableModifiers.Find(x => x.Name.Equals("maxKills"));
                if (oldMaxKillsModifier is not null)
                {
                    consumableModifiers.Remove(oldMaxKillsModifier);
                }

                consumableModifiers.Add(new("", "maxKills", maxKills, null, "ModifierConverter_MaxKillsAmount", Unit.None, new(), DisplayValueProcessingKind.None, ValueProcessingKind.None));

                var concealment = (decimal)plane.ConcealmentFromShips;
                var planesConcealment = (float)modifiers.ApplyModifiers("ConsumableDataContainer.Concealment.PCY012.PCY03", concealment);
                var oldConcealmentModifier = consumableModifiers.Find(x => x.Name.Equals("concealment"));
                if (oldConcealmentModifier is not null)
                {
                    consumableModifiers.Remove(oldConcealmentModifier);
                }

                consumableModifiers.Add(new("", "concealment", planesConcealment, null, "ShipStats_Concealment", Unit.KM, new(), DisplayValueProcessingKind.None, ValueProcessingKind.None));
            }
            else if (name.Contains("PCY018", StringComparison.InvariantCultureIgnoreCase))
            {
                // Torpedo Reload Booster
                uses = modifiers.ApplyModifiers("ConsumableDataContainer.Uses.PCY018", uses);

                cooldown = modifiers.ApplyModifiers("ConsumableDataContainer.Reload.PCY018", cooldown);
            }
            else if (name.Contains("PCY045", StringComparison.InvariantCultureIgnoreCase))
            {
                // Hydrophone
                consumableModifiers.UpdateConsumableModifierValue(modifiers, "ConsumableDataContainer.HydrophoneUpdateFrequency.PCY045", "hydrophoneUpdateFrequency");
            }
            else if (name.Contains("PCY048", StringComparison.InvariantCultureIgnoreCase))
            {
                // Submarine Surveillance
                prepTime = modifiers.ApplyModifiers("ConsumableDataContainer.PrepTime.PCY048", prepTime);
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
            Cooldown = Math.Round(cooldown, 1),
            PreparationTime = Math.Round(prepTime, 1),
            WorkTime = Math.Round(workTime, 1),
            Modifiers = consumableModifiers,
        };

        consumableDataContainer.UpdateDataElements();
        return consumableDataContainer;
    }
}
