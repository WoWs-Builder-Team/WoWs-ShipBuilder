using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record SpecialAbilityUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        [JsonIgnore]
        public string Description { get; set; } = default!;

        [DataUiUnit("S")]
        public decimal Duration { get; set; }

        public int RequiredHits { get; set; }

        [JsonIgnore]
        public Dictionary<string, float> Modifiers { get; set; } = null!;

        [JsonIgnore]
        public List<KeyValuePair<string, string>> SpecialData { get; set; } = default!;

        public static SpecialAbilityUI? FromShip(Ship ship, List<(string name, float value)> modifiers)
        {
            if (ship.SpecialAbility is null)
            {
                return null;
            }

            var specialAbility = ship.SpecialAbility;

            var specialUI = new SpecialAbilityUI
            {
                Name = $"DOCK_RAGE_MODE_TITLE_{specialAbility.Name}",
                Description = $"DOCK_RAGE_MODE_DESCRIPTION_{specialAbility.Name}",
                Duration = Math.Round((decimal)specialAbility.Duration, 1),
                RequiredHits = specialAbility.RequiredHits,
                Modifiers = specialAbility.Modifiers,
            };

            specialUI.SpecialData = specialUI.ToPropertyMapping();

            return specialUI;
        }
    }
}
