using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm
{
    public class CaptainSkillSelectorViewModel : ViewModelBase
    {
        private const int ArSkillNumber = 23;

        private const int ArSkillNumberSubs = 82;

        private const int FuriousSkillNumber = 81;

        private readonly ShipClass currentClass;

        private readonly Logger logger;

        public CaptainSkillSelectorViewModel()
            : this(ShipClass.Cruiser, LoadParamsAsync(DesktopAppDataService.PreviewInstance, new(), Nation.Usa).Result)
        {
        }

        public CaptainSkillSelectorViewModel(ShipClass shipClass, (Captain, Dictionary<string, Captain>?) vmParams, bool screenshotMode = false)
        {
            logger = Logging.GetLogger("CaptainSkillVM");
            ScreenshotMode = screenshotMode;
            currentClass = shipClass;

            var defaultCaptain = vmParams.Item1;

            // Rename Default Captain
            defaultCaptain.Name = Translation.CaptainSkillSelector_StandardCaptain;
            var capList = new Dictionary<string, Captain> { { Translation.CaptainSkillSelector_StandardCaptain, defaultCaptain } };

            var nationCaptains = vmParams.Item2;
            if (nationCaptains is {Count: > 0})
            {
                capList = capList.Union(nationCaptains).ToDictionary(x => x.Key, x => x.Value);
            }

            CaptainList = capList.Select(x => x.Value).ToList();
            SelectedCaptain = CaptainList.First();

            this.WhenAnyValue(x => x.AssignedPoints).Do(_ => UpdateCanAddSkill()).Subscribe();

            CaptainTalentsList.CollectionChanged += CaptainTalentsListOnCollectionChanged;
            ConditionalModifiersList.CollectionChanged += ConditionalModifiersListOnCollectionChanged;
        }

        private void CaptainTalentsListOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            TalentOrConditionalSkillEnabled = showArHpSelection && ArHpPercentage < 100 || CaptainTalentsList.Any(talent => talent.Status);
        }
        private void ConditionalModifiersListOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            TalentOrConditionalSkillEnabled = showArHpSelection && ArHpPercentage < 100 || ConditionalModifiersList.Any(skill => skill.Status);
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
                CaptainTalentsList.Clear();

                if (newCaptain!.UniqueSkills != null)
                {
                    CaptainWithTalents = true;
                    foreach ((string _, UniqueSkill talent) in newCaptain.UniqueSkills)
                    {
                        SkillActivationItemViewModel talentModel;

                        // get all the modifiers from the talents. workTime is excluded because it's for talents that automatically trigger a consumable, so it's not an effect we can show.
                        var modifiers = talent.SkillEffects.SelectMany(effect => effect.Value.Modifiers.Where(modifier => !modifier.Key.Equals("workTime"))).ToDictionary(x => x.Key, x => x.Value);
                        if (talent.MaxTriggerNum <= 1)
                        {
                            talentModel = new(talent.TranslationId, -1, modifiers, false, description: talent.TranslationId + "_DESCRIPTION");
                        }
                        else
                        {
                            talentModel = new(talent.TranslationId, -1, modifiers, false, talent.MaxTriggerNum, 1, talent.TranslationId + "_DESCRIPTION");
                        }

                        CaptainTalentsList.Add(talentModel);
                    }
                }
                else
                {
                    CaptainWithTalents = false;
                }

                var currentlySelectedNumbersList = SkillOrderList.Select(x => x.SkillNumber).ToList();
                SkillOrderList.Clear();
                foreach (var skillNumber in currentlySelectedNumbersList)
                {
                    var skill = SkillList.Values.Single(x => x.Skill.SkillNumber.Equals(skillNumber)).Skill;
                    SkillOrderList.Add(skill);
                }

                SkillActivationButtonEnabled = CaptainTalentsList.Count > 0 || ConditionalModifiersList.Count > 0 || ShowArHpSelection;
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
        [Obsolete("No longer needed as camos don't have economic bonuses anymore.")]
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

        private Dictionary<string, SkillItemViewModel>? skillList;

        /// <summary>
        /// Gets or sets the dictionary of skillList.
        /// </summary>
        public Dictionary<string, SkillItemViewModel>? SkillList
        {
            get => skillList;
            set => this.RaiseAndSetIfChanged(ref skillList, value);
        }

        /// <summary>
        /// Gets the List containing the selected skill in the order they were selected.
        /// </summary>
        public CustomObservableCollection<Skill> SkillOrderList { get; } = new();

        private bool showArHpSelection;

        /// <summary>
        /// Gets or sets a value indicating whether the hp bar slider for adrenaline rush should be shown.
        /// </summary>
        public bool ShowArHpSelection
        {
            get => showArHpSelection;
            set
            {
                this. RaiseAndSetIfChanged(ref showArHpSelection, value);
                TalentOrConditionalSkillEnabled = showArHpSelection && ArHpPercentage < 100 || CaptainTalentsList.Any(talent => talent.Status) || ConditionalModifiersList.Any(skill => skill.Status);
            }
        }

        private int arHpPercentage = 100;

        /// <summary>
        /// Gets or sets the current Adrenaline rush hp percentage.
        /// </summary>
        public int ArHpPercentage
        {
            get => arHpPercentage;
            set => this.RaiseAndSetIfChanged(ref arHpPercentage, value);
        }

        /// <summary>
        /// Gets the dictionary containing the conditional modifiers and their activation status.
        /// </summary>
        public CustomObservableCollection<SkillActivationItemViewModel> ConditionalModifiersList { get; } = new();

        /// <summary>
        /// Gets the dictionary containing the conditional modifiers and their activation status.
        /// </summary>
        public CustomObservableCollection<SkillActivationItemViewModel> CaptainTalentsList { get; } = new();

        private bool skillActivationPopupOpen;

        /// <summary>
        /// Gets or sets a value indicating whether the skill activation popup is visible.
        /// </summary>
        public bool SkillActivationPopupOpen
        {
            get => skillActivationPopupOpen;
            set
            {
                this.RaiseAndSetIfChanged(ref skillActivationPopupOpen, value);
                TalentOrConditionalSkillEnabled = showArHpSelection && ArHpPercentage < 100 || CaptainTalentsList.Any(talent => talent.Status) || ConditionalModifiersList.Any(skill => skill.Status);
            }
        }

        private bool skillActivationButtonEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether the skill activation button is enabled.
        /// </summary>
        public bool SkillActivationButtonEnabled
        {
            get => skillActivationButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref skillActivationButtonEnabled, value);
        }

        private bool talentOrConditionalSkillEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether a talent or a conditional skill are enabled.
        /// </summary>
        public bool TalentOrConditionalSkillEnabled
        {
            get => talentOrConditionalSkillEnabled;
            set => this.RaiseAndSetIfChanged(ref talentOrConditionalSkillEnabled, value);
        }

        private bool captainWithTalents;

        /// <summary>
        /// Gets or sets a value indicating whether the current captain has talents.
        /// </summary>
        public bool CaptainWithTalents
        {
            get => captainWithTalents;
            set => this.RaiseAndSetIfChanged(ref captainWithTalents, value);
        }

        public bool ScreenshotMode { get; }

        public static async Task<(Captain, Dictionary<string, Captain>?)> LoadParamsAsync(IAppDataService appDataService, AppSettings appSettings, Nation nation)
        {
            var defaultCaptain = (await appDataService.ReadLocalJsonData<Captain>(Nation.Common, appSettings.SelectedServerType))!.Single().Value;
            var nationCaptains = await appDataService.ReadLocalJsonData<Captain>(nation, appSettings.SelectedServerType);
            return (defaultCaptain, nationCaptains);
        }

        /// <summary>
        /// Get a <see><cref>Dictionary{string, Skill}</cref></see> for the class indicated by <paramref name="shipClass"/> from <paramref name="captain"/>.
        /// </summary>
        /// <param name="shipClass"> The <see cref="ShipClass"/> for which to take the skills.</param>
        /// <param name="captain"> The <see cref="Captain"/> from which to take the skills.</param>
        /// <returns>A dictionary containing the skill for the class from the captain.</returns>
        private Dictionary<string, SkillItemViewModel> GetSkillsForClass(ShipClass shipClass, Captain? captain)
        {
            logger.Info("Getting skill for class {0} from captain {1}", shipClass.ToString(), captain!.Name);
            var skills = captain.Skills;

            var filteredSkills = skills.Where(x => x.Value.LearnableOn.Contains(shipClass)).ToList();
            filteredSkills.ForEach(skill =>
            {
                // Get only the tier for the relevant class
                var classSkill = skill.Value.Tiers.Where(x => x.ShipClass == shipClass).ToList();
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
            Console.WriteLine(@"SKILLS: " + skills.Count);
            var filteredDictionary = filteredSkills.ToDictionary(x => x.Key, x => new SkillItemViewModel(x.Value, this));
            logger.Info("Found {0} skills", filteredDictionary.Count);
            return filteredDictionary;
        }

        /// <summary>
        /// Helper method to trigger a reevaluation of the <see cref="SkillItemViewModel.CanExecute"/> property of the skill view models.
        /// Also responsible for resetting the result cache after each evaluation.
        /// </summary>
        private void UpdateCanAddSkill()
        {
            if (SkillList == null)
            {
                return;
            }

            SkillItemViewModel.CanAddCache.Clear();
            SkillItemViewModel.CanRemoveCache.Clear();
            foreach (KeyValuePair<string, SkillItemViewModel> skill in SkillList)
            {
                skill.Value.CanExecuteChanged();
            }
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
                int pointCost = skill.Tiers.First().Tier + 1;
                AssignedPoints -= pointCost;
                if (skill.SkillNumber == ArSkillNumber || skill.SkillNumber == ArSkillNumberSubs)
                {
                    ShowArHpSelection = false;
                }

                if (skill.ConditionalModifiers is {Count: > 0})
                {
                    var skillName = SkillList!.Single(x => x.Value.Skill.Equals(skill)).Key;
                    ConditionalModifiersList.Remove(ConditionalModifiersList.Single(x => x.SkillName.Equals(skillName)));
                }

                this.RaisePropertyChanged(nameof(SkillOrderList));
            }
            else
            {
                SkillOrderList.Add(skill);
                var pointCost = skill.Tiers.First().Tier + 1;
                AssignedPoints += pointCost;
                if (skill.SkillNumber is ArSkillNumber or ArSkillNumberSubs)
                {
                    ShowArHpSelection = true;
                }

                if (skill.ConditionalModifiers is {Count: > 0})
                {
                    ConditionalModifiersList.Add(CreateItemViewModelForSkill(skill));
                }

                this.RaisePropertyChanged(nameof(SkillOrderList));
            }

            SkillActivationButtonEnabled = CaptainTalentsList.Count > 0 || ConditionalModifiersList.Count > 0 || ShowArHpSelection;
        }

        private SkillActivationItemViewModel CreateItemViewModelForSkill(Skill skill)
        {
            var skillName = SkillList!.Single(x => x.Value.Skill.Equals(skill)).Key;
            SkillActivationItemViewModel result;
            if (skill.SkillNumber is FuriousSkillNumber)
            {
                result = new(skillName, skill.SkillNumber, skill.ConditionalModifiers, false, 4);
            }
            else
            {
                result = new(skillName, skill.SkillNumber, skill.ConditionalModifiers, false);
            }

            return result;
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

            logger.Info("Reordering skills");

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

            logger.Info("Finished reordering skills");
        }

        /// <summary>
        /// Create a list of modifiers in a tuple format (name, value) from the currently selected skill.
        /// </summary>
        /// <returns>The List of modifiers of the currently selected skill.</returns>
        public List<(string, float)> GetModifiersList()
        {
            var modifiers = SkillOrderList.ToList()
                .Where(skill => skill.Modifiers != null && skill.SkillNumber != ArSkillNumber && skill.SkillNumber != ArSkillNumberSubs && skill.SkillNumber != FuriousSkillNumber)
                .SelectMany(m => m.Modifiers)
                .Select(effect => (effect.Key, effect.Value))
                .ToList();

            if (SkillOrderList.Any(skill => skill.SkillNumber == 14))
            {
                modifiers.Add(("fireResistanceEnabled", 1));
            }

            if (SkillOrderList.Any(skill => skill.SkillNumber == 22))
            {
                modifiers.Add(("interceptorSelected", 0));
            }

            if (ConditionalModifiersList.Count > 0)
            {
                var conditionalModifiers = ConditionalModifiersList.Where(skill => skill.Status && skill.SkillId != FuriousSkillNumber)
                    .SelectMany(skill => skill.Modifiers)
                    .Select(x => (x.Key, x.Value));

                modifiers.AddRange(conditionalModifiers);

                // Custom handling for Furious skill. Needs to take into account the number of fires
                var furiousSkill = SkillOrderList.SingleOrDefault(skill => skill.SkillNumber is FuriousSkillNumber);
                var furiousSkillModifier = ConditionalModifiersList.SingleOrDefault(skill => skill.SkillId is FuriousSkillNumber);
                if (furiousSkill is not null && furiousSkillModifier is not null && furiousSkillModifier.Status)
                {
                    var multiplier = (float)Math.Round(1 - (furiousSkillModifier.ActivationNumbers * (1 - furiousSkill.ConditionalModifiers["GMShotDelay"])), 2);
                    modifiers.Add(("GMShotDelay", multiplier));
                }
            }

            var arSkill = SkillOrderList.SingleOrDefault(skill => skill.SkillNumber is ArSkillNumber or ArSkillNumberSubs);
            if (arSkill is not null)
            {
                modifiers.Add(("lastChanceReloadCoefficient", arSkill.Modifiers["lastChanceReloadCoefficient"] * (100 - ArHpPercentage)));
            }

            if (CaptainTalentsList.Count > 0)
            {
                var talentModifiers = CaptainTalentsList.Where(talent => talent.Status && talent.MaximumActivations <= 1 && !talent.Modifiers.Any(modifier => modifier.Key.Equals("burnProbabilityBonus")))
                    .SelectMany(skill => skill.Modifiers)
                    .Select(x => (x.Key, x.Value));
                modifiers.AddRange(talentModifiers);

                var talentMultipleActivationModifiers = CaptainTalentsList.Where(talent => talent.Status && talent.MaximumActivations > 1 && !talent.Modifiers.Any(modifier => modifier.Key.Equals("burnProbabilityBonus")))
                    .SelectMany(talent => talent.Modifiers.Select(modifier => (modifier.Key, Value: Math.Pow(modifier.Value, talent.ActivationNumbers))))
                    .Select(x => (x.Key, (float)x.Value));
                modifiers.AddRange(talentMultipleActivationModifiers);

                var talentFireChanceModifier = CaptainTalentsList.Where(talent => talent.Status && talent.Modifiers.Any(modifier => modifier.Key.Equals("burnProbabilityBonus")))
                    .SelectMany(talent => talent.Modifiers.Select(modifier => (modifier.Key, Value: Math.Round(modifier.Value * talent.ActivationNumbers, 2))))
                    .Select(x => (x.Key, (float)x.Value));
                modifiers.AddRange(talentFireChanceModifier);
            }

            return modifiers;
        }

        /// <summary>
        /// Create a list of skill numbers from the currently selected list.
        /// </summary>
        /// <returns>The list of currently selected skill numbers.</returns>
        public List<int> GetSkillNumberList() => SkillOrderList.Select(skill => skill.SkillNumber).ToList();

        /// <summary>
        /// Return the index of the selected captain.
        /// </summary>
        /// <returns>The index of the selected captain.</returns>
        public string GetCaptainIndex()
        {
            return SelectedCaptain!.Index;
        }

        public void LoadBuild(List<int> selectedSkills, string? captainIndex)
        {
            // this check is purely for backward compatibility
            if (captainIndex != null)
            {
                var captain = CaptainList!.First(x => x.Index.Equals(captainIndex));
                SelectedCaptain = captain;
            }

            var skills = selectedSkills.Select(skillId => SelectedCaptain!.Skills.First(captainSkill => captainSkill.Value.SkillNumber == skillId)).Select(pair => pair.Value);
            SkillOrderList.AddRange(skills);
            AssignedPoints = SkillOrderList.Sum(skill => skill.Tiers.First().Tier + 1);
            foreach (var skill in SkillOrderList)
            {
                if (skill.SkillNumber is ArSkillNumber or ArSkillNumberSubs)
                {
                    ShowArHpSelection = true;
                }

                if (skill.ConditionalModifiers?.Any() ?? false)
                {
                    ConditionalModifiersList.Add(CreateItemViewModelForSkill(skill));
                }
            }

            SkillActivationButtonEnabled = CaptainTalentsList.Count > 0 || ConditionalModifiersList.Count > 0 || ShowArHpSelection;
        }
    }
}
