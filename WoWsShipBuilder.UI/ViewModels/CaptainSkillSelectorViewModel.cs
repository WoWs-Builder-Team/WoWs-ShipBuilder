using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Metadata;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    class CaptainSkillSelectorViewModel : ViewModelBase
    {
        public CaptainSkillSelectorViewModel(ShipClass shipClass)
        {
            SkillList = GetSkillsForClass(shipClass);
        }

        private int assignedPoints;

        public int AssignedPoints
        {
            get => assignedPoints;
            set => this.RaiseAndSetIfChanged(ref assignedPoints, value);
        }

        private Dictionary<string, Skill>? skillList;

        public Dictionary<string, Skill>? SkillList
        {
            get => skillList;
            set => this.RaiseAndSetIfChanged(ref skillList, value);
        }

        private List<Skill> skillOrderList = new();

        public List<Skill> SkillOrderList
        {
            get => skillOrderList;
            set => this.RaiseAndSetIfChanged(ref skillOrderList, value);
        }

        private static Dictionary<string, Skill> GetSkillsForClass(ShipClass shipClass, string captainId = "PCW001_CrewCommon")
        {
            var captain = AppDataHelper.Instance.ReadLocalJsonData<Captain>(null, ServerType.Live);
            var skills = captain![captainId].Skills;
            var filteredSkills = skills.Where(x => x.Value.LearnableOn.Contains(shipClass)).ToList();
            filteredSkills.ForEach(skill =>
            {
                var classSkill = skill.Value.Tiers.Where(x => x.ShipClass == shipClass).ToList();
                var first = classSkill.First();
                skill.Value.Tiers = classSkill;
            });
            var filteredDictionary = filteredSkills.ToDictionary(x => x.Key, x => x.Value);
            return filteredDictionary;
        }

        public void AddSkill(Skill skill)
        {
            if (SkillOrderList.Contains(skill))
            {
                SkillOrderList.Remove(skill);
                ReorderSkillList();
                var pointCost = skill.Tiers.First().Tier + 1;
                AssignedPoints -= pointCost;
                this.RaisePropertyChanged(nameof(SkillOrderList));
            }
            else
            {
                SkillOrderList.Add(skill);
                var pointCost = skill.Tiers.First().Tier + 1;
                AssignedPoints += pointCost;
                this.RaisePropertyChanged(nameof(SkillOrderList));
            }
        }

        [DependsOn(nameof(SkillOrderList))]
        public bool CanAddSkill(object parameter)
        {
            if (parameter is Skill skill)
            {
                // Get cost of the skill i'm trying to add
                var currentSkillTier = skill.Tiers.First().Tier;

                // Adding skill
                if (!SkillOrderList.Contains(skill))
                {
                    // If the points would go over 21, can't add the skill
                    if (AssignedPoints + currentSkillTier + 1 > 21)
                    {
                        return false;
                    }

                    // If it's a skill of the first tier, i can always add it
                    if (currentSkillTier == 0)
                    {
                        return true;
                    }

                    // If it's not, i search the skill of the previous tier. If at least one exist, i can add it
                    List<int>? previousTierSkills = SkillOrderList.Select(iteratedSkill => iteratedSkill.Tiers.First().Tier).Where(skillCost => skillCost == currentSkillTier - 1).ToList();

                    if (previousTierSkills != null && previousTierSkills.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                // Removing skill
                else
                {
                    // If the skill tier is 4 (tier 3, since our index start from 0), can always remove
                    if (currentSkillTier == 3)
                    {
                        return true;
                    }

                    var nextTierSkills = SkillOrderList.Select(iteratedSkill => iteratedSkill.Tiers.First().Tier).Where(skillCost => skillCost > currentSkillTier).ToList();

                    var sameTierSkills = SkillOrderList.Select(iteratedSkill => iteratedSkill.Tiers.First().Tier).Where(skillCost => skillCost == currentSkillTier).ToList();

                    if (nextTierSkills.Count > 0 && sameTierSkills.Count > 1)
                    {
                        return true;
                    }
                    else if (nextTierSkills.Count > 0 && sameTierSkills.Count == 1)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ReorderSkillList()
        {
            if (SkillOrderList.Count == 0)
            {
                return;
            }

            var groups = SkillOrderList.GroupBy(skill => skill.Tiers.First().Tier).Select(x => x.ToList()).ToList().OrderBy(x => x.First().Tiers.First().Tier).ToList();

            // Tier 0 skill reordering
            var tier0Skills = groups[0];

            var tier0SkillsFirst = false;
            foreach (var tier0Skill in tier0Skills)
            {
                if (skillOrderList.IndexOf(tier0Skill) == 0)
                {
                    tier0SkillsFirst = true;
                }
            }

            if (!tier0SkillsFirst)
            {
                var firstTier0Skill = tier0Skills.First();
                SkillOrderList.Remove(firstTier0Skill);
                SkillOrderList.Insert(0, firstTier0Skill);
            }

            // Tier 1 skill reordering
            if (groups.Count > 2)
            {
                var tier1Skills = groups[1];
                var firstTier0SkillIndex = SkillOrderList.FindIndex(skill => skill.Tiers.First().Tier == 0);
                var firstTier2SkillIndex = SkillOrderList.FindIndex(skill => skill.Tiers.First().Tier == 2);

                var tier1SkillFirst = false;

                foreach (var tier1Skill in tier1Skills)
                {
                    if (SkillOrderList.IndexOf(tier1Skill) > firstTier0SkillIndex && SkillOrderList.IndexOf(tier1Skill) < firstTier2SkillIndex)
                    {
                        tier1SkillFirst = true;
                    }
                }

                if (!tier1SkillFirst)
                {
                    var skill = tier1Skills.First();
                    SkillOrderList.Remove(skill);
                    SkillOrderList.Insert(firstTier2SkillIndex, skill);
                }
            }

            // Tier 2 skill reordering
            if (groups.Count > 3)
            {
                var tier1Skills = groups[2];
                var firstTier1SkillIndex = SkillOrderList.FindIndex(skill => skill.Tiers.First().Tier == 1);
                var firstTier3SkillIndex = SkillOrderList.FindIndex(skill => skill.Tiers.First().Tier == 3);

                var tier1SkillFirst = false;

                foreach (var tier1Skill in tier1Skills)
                {
                    if (SkillOrderList.IndexOf(tier1Skill) > firstTier1SkillIndex && SkillOrderList.IndexOf(tier1Skill) < firstTier3SkillIndex)
                    {
                        tier1SkillFirst = true;
                    }
                }

                if (!tier1SkillFirst)
                {
                    var skill = tier1Skills.First();
                    SkillOrderList.Remove(skill);
                    SkillOrderList.Insert(firstTier3SkillIndex, skill);
                }
            }
        }
    }
}
