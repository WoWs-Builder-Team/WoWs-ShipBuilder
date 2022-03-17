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

        private readonly int skillTier;

        private bool canExecute;

        public SkillItemViewModel(Skill skill, CaptainSkillSelectorViewModel parent)
        {
            Skill = skill;
            skillTier = skill.Tiers.First().Tier;
            this.parent = parent;
            CanExecuteChanged();
        }

        /// <summary>
        /// Gets a dictionary that can be used to share the result of the <see cref="CanExecuteChanged"/> check for skills that are not currently selected.
        /// </summary>
        public static Dictionary<int, bool> CanAddCache { get; } = new();

        public static Dictionary<int, bool> CanRemoveCache { get; } = new();

        public Skill Skill { get; }

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
                if (CanAddCache.TryGetValue(skillTier, out bool canAdd))
                {
                    CanExecute = canAdd;
                    return;
                }

                // If the points would go over 21, can't add the skill
                if (parent.AssignedPoints + skillTier + 1 > 21)
                {
                    result = false;
                }

                // If it's a skill of the first tier, i can always add it
                else if (skillTier == 0)
                {
                    result = true;
                }
                else
                {
                    // If it's not, i search the skill of the previous tier. If at least one exist, i can add it
                    result = parent.SkillOrderList.Select(s => s.Tiers.First().Tier).Any(cost => cost == skillTier - 1);
                }

                CanAddCache[skillTier] = result;
            }

            // Can remove skill
            else
            {
                // If the skill tier is 4 (tier 3, since our index start from 0), can always remove
                if (skillTier == 3)
                {
                    result = true;
                }
                else
                {
                    if (CanRemoveCache.TryGetValue(skillTier, out bool canRemove))
                    {
                        CanExecute = canRemove;
                        return;
                    }

                    List<int> skillTiers = parent.SkillOrderList.Select(iteratedSkill => iteratedSkill.Tiers.First().Tier).ToList();
                    int nextTierSkillCount = skillTiers.Count(skillCost => skillCost > skillTier);
                    int sameTierSkillCount = skillTiers.Count(skillCost => skillCost == skillTier);

                    if (nextTierSkillCount > 0)
                    {
                        result = sameTierSkillCount > 1;
                    }
                    else
                    {
                        result = true;
                    }

                    CanRemoveCache[skillTier] = result;
                }
            }

            CanExecute = result;
        }
    }
}
