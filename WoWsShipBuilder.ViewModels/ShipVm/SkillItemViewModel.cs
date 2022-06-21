using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm
{
    public class SkillItemViewModel : ViewModelBase
    {
        private readonly CaptainSkillSelectorViewModel parent;

        private bool canExecute;

        private readonly ShipClass shipClass;

        public SkillItemViewModel(Skill skill, CaptainSkillSelectorViewModel parent, ShipClass shipClass)
        {
            Skill = skill;
            SkillTier = skill.Tiers.First(x => x.ShipClass == shipClass).Tier;
            SkillXPosition = skill.Tiers.First(x => x.ShipClass == shipClass).XPosition;
            if (skill.Modifiers is not null)
            {
                Modifiers = skill.Modifiers.Where(x => !x.Key.Contains('_') || x.Key.Contains("_" + shipClass)).ToDictionary(x => x.Key, x => x.Value);
            }

            if (skill.ConditionalModifiers is not null)
            {
                ConditionalModifiers = skill.ConditionalModifiers.Where(x => !x.Key.Contains('_') || x.Key.Contains("_" + shipClass)).ToDictionary(x => x.Key, x => x.Value);
            }
            this.shipClass = shipClass;
            this.parent = parent;
            CanExecuteChanged();
        }

        /// <summary>
        /// Gets a dictionary that can be used to share the result of the <see cref="CanExecuteChanged"/> check for skills that are not currently selected.
        /// </summary>
        public static Dictionary<int, bool> CanAddCache { get; } = new();

        public static Dictionary<int, bool> CanRemoveCache { get; } = new();

        public Skill Skill { get; }

        public int SkillTier { get; }

        public int SkillXPosition { get; }

        public Dictionary<string, float> Modifiers { get; } = new();

        public Dictionary<string, float> ConditionalModifiers { get; } = new();

        public bool CanExecute
        {
            get => canExecute;
            set => this.RaiseAndSetIfChanged(ref canExecute, value);
        }

        public void CanExecuteChanged()
        {
            bool result;

            // Can add skill
            if (!parent.SkillOrderList.Contains(Skill))
            {
                if (CanAddCache.TryGetValue(SkillTier, out bool canAdd))
                {
                    CanExecute = canAdd;
                    return;
                }

                // If the points would go over 21, can't add the skill
                if (parent.AssignedPoints + SkillTier + 1 > 21)
                {
                    result = false;
                }

                // If it's a skill of the first tier, i can always add it
                else if (SkillTier == 0)
                {
                    result = true;
                }
                else
                {
                    // If it's not, i search the skill of the previous tier. If at least one exist, i can add it
                    result = parent.SkillOrderList.Select(s => s.Tiers.First(x => x.ShipClass == shipClass).Tier).Any(cost => cost == SkillTier - 1);
                }

                CanAddCache[SkillTier] = result;
            }

            // Can remove skill
            else
            {
                // If the skill tier is 4 (tier 3, since our index start from 0), can always remove
                if (SkillTier == 3)
                {
                    result = true;
                }
                else
                {
                    if (CanRemoveCache.TryGetValue(SkillTier, out bool canRemove))
                    {
                        CanExecute = canRemove;
                        return;
                    }

                    List<int> skillTiers = parent.SkillOrderList.Select(iteratedSkill => iteratedSkill.Tiers.First(x => x.ShipClass == shipClass).Tier).ToList();
                    int nextTierSkillCount = skillTiers.Count(skillCost => skillCost > SkillTier);
                    int sameTierSkillCount = skillTiers.Count(skillCost => skillCost == SkillTier);

                    if (nextTierSkillCount > 0)
                    {
                        result = sameTierSkillCount > 1;
                    }
                    else
                    {
                        result = true;
                    }

                    CanRemoveCache[SkillTier] = result;
                }
            }

            CanExecute = result;
        }
    }
}
