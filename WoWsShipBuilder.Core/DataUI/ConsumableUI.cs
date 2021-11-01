using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;

namespace WoWsShipBuilder.Core.DataUI
{
    public record ConsumableUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        [JsonIgnore]
        public string IconName { get; set; } = default!;

        [JsonIgnore]
        public int Slot { get; set; }

        [JsonIgnore]
        public string Desc { get; set; } = default!;

        public string NumberOfUses { get; set; } = default!;

        [DataUiUnit("S")]
        public decimal Cooldown { get; set; }

        [DataUiUnit("S")]
        public decimal WorkTime { get; set; }

        [JsonIgnore]
        public Dictionary<string, float> Modifiers { get; set; } = null!;

        [JsonIgnore]
        public List<KeyValuePair<string, string>> ConsumableData { get; set; } = default!;

        public static ConsumableUI FromTypeAndVariant(string name, string variant, int slot, List<(string name, float value)> modifiers, bool isCvPlanes)
        {
            var consumable = AppData.ConsumableList![$"{name} {variant}"];

            var iconName = consumable.IconId;
            if (string.IsNullOrEmpty(iconName))
            {
                iconName = name;
            }

            var localizationKey = consumable.DescId;
            if (string.IsNullOrEmpty(localizationKey))
            {
                localizationKey = consumable.Name;
            }

            int uses = consumable.NumConsumables;
            float cooldown = consumable.ReloadTime;
            float workTime = consumable.WorkTime;
            if (isCvPlanes)
            {
                if (name.Contains("PCY036", StringComparison.InvariantCultureIgnoreCase))
                {
                    var usesModifiers = modifiers.FindModifiers("regenerateHealthAdditionalConsumables");
                    uses = usesModifiers.Aggregate(uses, (current, modifier) => (int)(current + modifier));
                }
                else if (name.Contains("PCY035", StringComparison.InvariantCultureIgnoreCase))
                {
                    var usesModifiers = modifiers.FindModifiers("callFightersAdditionalConsumables");
                    uses = usesModifiers.Aggregate(uses, (current, modifier) => (int)(current + modifier));
                }
            }
            else
            {
                var usesModifiers = modifiers.FindModifiers("additionalConsumables");
                uses = usesModifiers.Aggregate(uses, (current, modifier) => (int)(current + modifier));

                var allCooldownModifiers = modifiers.FindModifiers("ConsumableReloadTime");
                cooldown = allCooldownModifiers.Aggregate(cooldown, (current, modifier) => (current * modifier));

                var allWorkModifiers = modifiers.FindModifiers("ConsumablesWorkTime");
                workTime = allWorkModifiers.Aggregate(workTime, (current, modifier) => (current * modifier));

                if (name.Contains("PCY003", StringComparison.InvariantCultureIgnoreCase))
                {
                    var cooldownModifiers = modifiers.FindModifiers("airDefenseDispReloadCoeff");
                    cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);

                    var workTimeModifiers = modifiers.FindModifiers("airDefenseDispWorkTimeCoeff");
                    workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
                }
                else if (name.Contains("PCY005", StringComparison.InvariantCultureIgnoreCase))
                {
                    var cooldownModifiers = modifiers.FindModifiers("scoutReloadCoeff");
                    cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);

                    var workTimeModifiers = modifiers.FindModifiers("scoutWorkTimeCoeff");
                    workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
                }
                else if (name.Contains("PCY010", StringComparison.InvariantCultureIgnoreCase))
                {
                    var workTimeModifiers = modifiers.FindModifiers("regenCrewWorkTimeCoeff");
                    workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
                }
                else if (name.Contains("PCY016", StringComparison.InvariantCultureIgnoreCase))
                {
                    var workTimeModifiers = modifiers.FindModifiers("sonarWorkTimeCoeff");
                    workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
                }
                else if (name.Contains("PCY032", StringComparison.InvariantCultureIgnoreCase))
                {
                    var workTimeModifiers = modifiers.FindModifiers("rlsWorkTimeCoeff");
                    workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
                }
                else if (name.Contains("PCY023", StringComparison.InvariantCultureIgnoreCase))
                {
                    var workTimeModifiers = modifiers.FindModifiers("crashCrewWorkTimeCoeff");
                    workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
                }
                else if (name.Contains("PCY028", StringComparison.InvariantCultureIgnoreCase))
                {
                    var workTimeModifiers = modifiers.FindModifiers("smokeGeneratorWorkTimeCoeff");
                    workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
                }
                else if (name.Contains("PCY015", StringComparison.InvariantCultureIgnoreCase))
                {
                    var workTimeModifiers = modifiers.FindModifiers("speedBoostersWorkTimeCoeff");
                    workTime = workTimeModifiers.Aggregate(workTime, (current, modifier) => current * modifier);
                }
                else if (name.Contains("PCY033", StringComparison.InvariantCultureIgnoreCase))
                {
                    var cooldownModifiers = modifiers.FindModifiers("artilleryBoostersReloadCoeff");
                    cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);
                }
                else if (name.Contains("PCY026", StringComparison.InvariantCultureIgnoreCase))
                {
                    var cooldownModifiers = modifiers.FindModifiers("fighterReloadCoeff");
                    cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);
                }
                else if (name.Contains("PCY031", StringComparison.InvariantCultureIgnoreCase))
                {
                    var cooldownModifiers = modifiers.FindModifiers("torpedoReloaderReloadCoeff");
                    cooldown = cooldownModifiers.Aggregate(cooldown, (current, modifier) => current * modifier);
                }
            }

            var numberOfUses = uses.ToString();
            if (consumable.NumConsumables == -1)
            {
                numberOfUses = "∞";
            }

            var consumableUI = new ConsumableUI
            {
                Name = localizationKey,
                NumberOfUses = numberOfUses,
                IconName = iconName,
                Slot = slot,
                Desc = "",
                Cooldown = Math.Round((decimal)cooldown, 1),
                WorkTime = Math.Round((decimal)workTime, 1),
                Modifiers = consumable.Modifiers ?? new Dictionary<string, float>(),
            };

            consumableUI.ConsumableData = consumableUI.ToPropertyMapping();

            return consumableUI;
        }
    }
}
