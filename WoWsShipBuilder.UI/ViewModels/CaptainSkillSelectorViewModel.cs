using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Metadata;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class CaptainSkillSelectorViewModel : ViewModelBase
    {
        private readonly ShipClass currentClass;

        // TODO: unify constructors
        public CaptainSkillSelectorViewModel(ShipClass shipClass, Nation nation)
        {
            var captainList = AppDataHelper.Instance.ReadLocalJsonData<Captain>(Nation.Common, AppData.Settings.SelectedServerType);
            var nationCaptain = AppDataHelper.Instance.ReadLocalJsonData<Captain>(nation, AppData.Settings.SelectedServerType);
            if (nationCaptain != null && nationCaptain.Count > 0)
            {
                captainList = captainList!.Union(nationCaptain).ToDictionary(x => x.Key, x => x.Value);
            }

            currentClass = shipClass;
            CaptainList = captainList!.Select(x => x.Value).ToList();
            SelectedCaptain = CaptainList.First();
        }

        public CaptainSkillSelectorViewModel(ShipClass shipClass, Nation nation, List<int> selectedSkills)
        {
            var captainList = AppDataHelper.Instance.ReadLocalJsonData<Captain>(Nation.Common, AppData.Settings.SelectedServerType);
            var nationCaptain = AppDataHelper.Instance.ReadLocalJsonData<Captain>(nation, AppData.Settings.SelectedServerType);
            if (nationCaptain != null && nationCaptain.Count > 0)
            {
                captainList = captainList!.Union(nationCaptain).ToDictionary(x => x.Key, x => x.Value);
            }

            currentClass = shipClass;
            CaptainList = captainList!.Select(x => x.Value).ToList();
            SelectedCaptain = CaptainList.First();
            var skills = selectedSkills.Select(skillId => SelectedCaptain.Skills.First(captainSkill => captainSkill.Value.SkillNumber == skillId)).Select(pair => pair.Value);
            SkillOrderList.AddRange(skills);
            AssignedPoints = SkillOrderList.Sum(skill => skill.Tiers.First().Tier + 1);
        }

        private Captain? selectedCaptain;

        /// <summary>
        /// Gets or sets the currently selected captain and update the skills associated with it.
        /// </summary>
        public Captain? SelectedCaptain
        {
            get => selectedCaptain;
            set
            {
                var newCaptain = value ?? selectedCaptain;
                this.RaiseAndSetIfChanged(ref selectedCaptain, newCaptain);
                SkillList = GetSkillsForClass(currentClass, newCaptain);
                var currentlySelectednumbersList = SkillOrderList.Select(x => x.SkillNumber).ToList();
                SkillOrderList.Clear();
                foreach (var skillNumber in currentlySelectednumbersList)
                {
                    var skill = SkillList.Values.Single(x => x.SkillNumber.Equals(skillNumber));
                    SkillOrderList.Add(skill);
                }
            }
        }

        private List<Captain>? captainList;

        /// <summary>
        /// Gets or sets the list of available captains.
        /// </summary>
        public List<Captain>? CaptainList
        {
            get => captainList;
            set => this.RaiseAndSetIfChanged(ref captainList, value);
        }

        private bool camoEnabled = true;

        /// <summary>
        /// Gets or sets a value indicating whether the camo bonuses should be applied.
        /// </summary>
        public bool CamoEnabled
        {
            get => camoEnabled;
            set => this.RaiseAndSetIfChanged(ref camoEnabled, value);
        }

        private int assignedPoints;

        /// <summary>
        /// Gets or sets the number of assigned skill points.
        /// </summary>
        public int AssignedPoints
        {
            get => assignedPoints;
            set => this.RaiseAndSetIfChanged(ref assignedPoints, value);
        }

        private Dictionary<string, Skill>? skillList;

        /// <summary>
        /// Gets or sets the dictionary of skillList.
        /// </summary>
        public Dictionary<string, Skill>? SkillList
        {
            get => skillList;
            set => this.RaiseAndSetIfChanged(ref skillList, value);
        }

        private AvaloniaList<Skill> skillOrderList = new();

        /// <summary>
        /// Gets or sets the List containing the selected skill in the order they were selected.
        /// </summary>
        public AvaloniaList<Skill> SkillOrderList
        {
            get => skillOrderList;
            set => this.RaiseAndSetIfChanged(ref skillOrderList, value);
        }

        /// <summary>
        /// Get a <see cref="Dictionary{string, Skill}"/> for the class indicated by <paramref name="shipClass"/> from <paramref name="captain"/>.
        /// </summary>
        /// <param name="shipClass"> The <see cref="ShipClass"/> for which to take the skills.</param>
        /// <param name="captain"> The <see cref="Captain"/> from which to take the skills.</param>
        /// <returns>A dictionary containg the skill for the class from the captain.</returns>
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

        /// <summary>
        /// Add a skill to <see cref="SkillOrderList"/>.
        /// </summary>
        /// <param name="skill"> the <see cref="Skill"/> to add.</param>
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

        /// <summary>
        /// Check if a certain <see cref="Skill"/> can be added to <see cref="SkillOrderList"/>.
        /// </summary>
        /// <param name="parameter"> The <see cref="Skill"/> object to add to <see cref="SkillOrderList"/>.</param>
        /// <returns> If the <see cref="Skill"/> can be added.</returns>
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

        /// <summary>
        /// Reorder the skills in <see cref="SkillOrderList"/> to make the skill order possible.
        /// It works with the assumption that if the first skill of a certain tier got removed, it should be replaced by the first skill of the same tier selected by the user.
        /// </summary>
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

        /// <summary>
        /// Create a list of modifiers in a tuple format (name, value) from the currently selected skill.
        /// </summary>
        /// <returns>The List of modifiers of the currently selected skill.</returns>
        public List<(string, float)> GetModifiersList()
        {
            var modifiers = SkillOrderList.Where(skill => skill.Modifiers != null)
               .SelectMany(m => m.Modifiers).Select(effect => (effect.Key, effect.Value))
               .ToList();

            if (CamoEnabled)
            {
                modifiers.Add(("visibilityFactor", 0.97f));
            }

            return modifiers;
        }

        /// <summary>
        /// Create a list of skill numbers from the currently selected list.
        /// </summary>
        /// <returns>The list of currently selected skill numbers.</returns>
        public List<int> GetSkillNumberList()
        {
            var list = SkillOrderList.Select(skill => skill.SkillNumber).ToList();
            return list;
        }
    }
}
