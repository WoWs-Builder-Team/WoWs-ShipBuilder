using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Metadata;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class CaptainSkillSelectorViewModel : ViewModelBase
    {
        private readonly ShipClass currentClass;

        public CaptainSkillSelectorViewModel(ShipClass shipClass, Nation nation)
        {
            var captainList = AppDataHelper.Instance.ReadLocalJsonData<Captain>(Nation.Common, AppData.Settings.SelectedServerType);
            var nationCaptain = AppDataHelper.Instance.ReadLocalJsonData<Captain>(nation, ServerType.Live);
            if (nationCaptain != null && nationCaptain.Count > 0)
            {
                captainList = captainList!.Union(nationCaptain).ToDictionary(x => x.Key, x => x.Value);
            }

            currentClass = shipClass;
            CaptainList = captainList!.Select(x => x.Value).ToList();
            SelectedCaptain = CaptainList.First();
        }

        private Captain? selectedCaptain;

        public Captain? SelectedCaptain
        {
            get => selectedCaptain;
            set
            {
                var newCaptain = value ?? selectedCaptain;
                this.RaiseAndSetIfChanged(ref selectedCaptain, newCaptain);
                var currentlySelectedList = SkillOrderList.Select(x => x.SkillNumber).ToList();
                SkillList = GetSkillsForClass(currentClass, newCaptain);
                SkillOrderList = new();
                AssignedPoints = 0;
            }
        }

        private List<Captain>? captainList;

        public List<Captain>? CaptainList
        {
            get => captainList;
            set => this.RaiseAndSetIfChanged(ref captainList, value);
        }

        private bool camoEnabled = true;

        public bool CamoEnabled
        {
            get => camoEnabled;
            set => this.RaiseAndSetIfChanged(ref camoEnabled, value);
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

        private AvaloniaList<Skill> skillOrderList = new();

        public AvaloniaList<Skill> SkillOrderList
        {
            get => skillOrderList;
            set => this.RaiseAndSetIfChanged(ref skillOrderList, value);
        }

        private Dictionary<string, Skill> GetSkillsForClass(ShipClass shipClass, Captain? captain)
        {
            var skills = captain!.Skills;

            var filteredSkills = skills.Where(x => x.Value.LearnableOn.Contains(shipClass)).ToList();
            filteredSkills.ForEach(skill =>
            {
                // Get only the tier for the relevant class
                var classSkill = skill.Value.Tiers.Where(x => x.ShipClass == shipClass).ToList();
                var first = classSkill.First();
                skill.Value.Tiers = classSkill;

                // Get only the value for the relevant class
                Dictionary<string, float> modifierClassSkill = skill.Value.Modifiers
                    ?.Where(x => x.Key.Contains(shipClass.ToString()))
                    .ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, float>();

                // If Count is 0, the skill has universal value. If Count is > 0, the skill has values divided by class
                if (modifierClassSkill.Count > 0)
                {
                    skill.Value.Modifiers = modifierClassSkill;
                }
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

            // AvaloniaList is missing one of the extension methods, so we copy the list to a normal one,
            var supportList = SkillOrderList.ToList();
            var groups = SkillOrderList.GroupBy(skill => skill.Tiers.First().Tier).Select(x => x.ToList()).ToList().OrderBy(x => x.First().Tiers.First().Tier).ToList();

            // Tier 0 skill reordering
            var tier0Skills = groups[0];

            var tier0SkillsFirst = false;
            foreach (var tier0Skill in tier0Skills)
            {
                if (SkillOrderList.IndexOf(tier0Skill) == 0)
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
                var firstTier0SkillIndex = supportList.FindIndex(skill => skill.Tiers.First().Tier == 0);
                var firstTier2SkillIndex = supportList.FindIndex(skill => skill.Tiers.First().Tier == 2);

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
                var firstTier1SkillIndex = supportList.FindIndex(skill => skill.Tiers.First().Tier == 1);
                var firstTier3SkillIndex = supportList.FindIndex(skill => skill.Tiers.First().Tier == 3);

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

        public List<(string, float)> GetModifiersList()
        {
           return SkillOrderList.SelectMany(m => m.Modifiers.Select(effect => (effect.Key, (float)effect.Value)))
                .ToList();
        }
    }
}
