using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Localization.Resources;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public partial class CaptainSkillSelectorViewModel : ReactiveObject
{
    private const int ArSkillNumber = 23;

    private const int ArSkillNumberSubs = 82;

    private const int FuriousSkillNumber = 81;

    private const int ImprovedRepairPartyReadinessSkillNumber = 44;

    private readonly Dictionary<int, bool> canAddSkillCache = new();

    private readonly Dictionary<int, bool> canRemoveSkillCache = new();

    private readonly ShipClass currentClass;

    private readonly ILogger<CaptainSkillSelectorViewModel> logger;

    [Observable]
    private int arHpPercentage = 100;

    [Observable]
    private int assignedPoints;

    [Observable]
    private List<Captain>? captainList;

    [Observable]
    private bool captainWithTalents;

    private Captain? selectedCaptain;

    private bool showArHpSelection;

    [Observable]
    private bool skillActivationButtonEnabled;

    private bool skillActivationPopupOpen;

    [Observable]
    private Dictionary<string, SkillItemViewModel>? skillList;

    [Observable]
    private bool talentOrConditionalSkillEnabled;

    public CaptainSkillSelectorViewModel()
        : this(ShipClass.Cruiser, LoadParams(Nation.Usa))
    {
    }

    public CaptainSkillSelectorViewModel(ShipClass shipClass, (Captain defaultCaptain, Dictionary<string, Captain>? captainList) vmParams)
    {
        this.logger = Logging.LoggerFactory.CreateLogger<CaptainSkillSelectorViewModel>();
        this.currentClass = shipClass;

        var defaultCaptain = vmParams.defaultCaptain;

        // Rename Default Captain
        defaultCaptain.Name = Translation.CaptainSkillSelector_StandardCaptain;
        var capList = new Dictionary<string, Captain> { { Translation.CaptainSkillSelector_StandardCaptain, defaultCaptain } };

        var nationCaptains = vmParams.captainList;
        if (nationCaptains is { Count: > 0 })
        {
            capList = capList.Union(nationCaptains).ToDictionary(x => x.Key, x => x.Value);
        }

        this.CaptainList = capList.Select(x => x.Value).ToList();
        this.SelectedCaptain = this.CaptainList[0];

        this.WhenAnyValue(x => x.AssignedPoints).Do(_ => this.UpdateCanAddSkill()).Subscribe();

        this.CaptainTalentsList.CollectionChanged += this.CaptainTalentsListOnCollectionChanged;
        this.ConditionalModifiersList.CollectionChanged += this.ConditionalModifiersListOnCollectionChanged;
    }

    /// <summary>
    /// Gets or sets the currently selected captain and update the skills associated with it.
    /// </summary>
    public Captain? SelectedCaptain
    {
        get => this.selectedCaptain;
        set
        {
            var newCaptain = value ?? this.selectedCaptain;
            this.RaiseAndSetIfChanged(ref this.selectedCaptain, newCaptain);
            this.SkillList = this.ConvertSkillToViewModel(this.currentClass, newCaptain);
            this.CaptainTalentsList.Clear();

            if (newCaptain!.UniqueSkills.Any())
            {
                this.CaptainWithTalents = true;
                foreach ((string _, UniqueSkill talent) in newCaptain.UniqueSkills)
                {
                    SkillActivationItemViewModel talentModel;

                    // get all the modifiers from the talents. workTime is excluded because it's for talents that automatically trigger a consumable, so it's not an effect we can show.
                    var modifiers = talent.SkillEffects.SelectMany(effect => effect.Value.Modifiers.Where(modifier => !modifier.Name.Equals("workTime", StringComparison.Ordinal))).ToImmutableList();
                    if (talent.MaxTriggerNum <= 1)
                    {
                        talentModel = new(talent.TranslationId, -1, modifiers, false, description: talent.TranslationId + "_DESCRIPTION");
                    }
                    else
                    {
                        talentModel = new(talent.TranslationId, -1, modifiers, false, talent.MaxTriggerNum, 1, talent.TranslationId + "_DESCRIPTION");
                    }

                    this.CaptainTalentsList.Add(talentModel);
                }
            }
            else
            {
                this.CaptainWithTalents = false;
            }

            var currentlySelectedNumbersList = this.SkillOrderList.Select(x => x.SkillNumber).ToList();
            this.SkillOrderList.Clear();
            foreach (var skillNumber in currentlySelectedNumbersList)
            {
                var skill = this.SkillList.Values.Single(x => x.Skill.SkillNumber.Equals(skillNumber)).Skill;
                this.SkillOrderList.Add(skill);
            }

            this.UpdateCanAddSkill(); // Needs to be called to update the internal state of the skill viewmodels
            this.SkillActivationButtonEnabled = this.CaptainTalentsList.Count > 0 || this.ConditionalModifiersList.Count > 0 || this.ShowArHpSelection;
        }
    }

    /// <summary>
    /// Gets the List containing the selected skill in the order they were selected.
    /// </summary>
    public CustomObservableCollection<Skill> SkillOrderList { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the hp bar slider for adrenaline rush should be shown.
    /// </summary>
    public bool ShowArHpSelection
    {
        get => this.showArHpSelection;
        set
        {
            this.RaiseAndSetIfChanged(ref this.showArHpSelection, value);
            this.TalentOrConditionalSkillEnabled = (this.showArHpSelection && this.ArHpPercentage < 100) || this.CaptainTalentsList.Any(talent => talent.Status) || this.ConditionalModifiersList.Any(skill => skill.Status);
        }
    }

    /// <summary>
    /// Gets the dictionary containing the conditional modifiers and their activation status.
    /// </summary>
    public CustomObservableCollection<SkillActivationItemViewModel> ConditionalModifiersList { get; } = new();

    /// <summary>
    /// Gets the dictionary containing the conditional modifiers and their activation status.
    /// </summary>
    public CustomObservableCollection<SkillActivationItemViewModel> CaptainTalentsList { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the skill activation popup is visible.
    /// </summary>
    public bool SkillActivationPopupOpen
    {
        get => this.skillActivationPopupOpen;
        set
        {
            this.RaiseAndSetIfChanged(ref this.skillActivationPopupOpen, value);
            this.TalentOrConditionalSkillEnabled = (this.showArHpSelection && this.ArHpPercentage < 100) || this.CaptainTalentsList.Any(talent => talent.Status) || this.ConditionalModifiersList.Any(skill => skill.Status);
        }
    }

    public static (Captain, Dictionary<string, Captain>?) LoadParams(Nation nation)
    {
        var defaultCaptain = AppData.CaptainCache[Nation.Common].Single().Value;
        var nationCaptains = AppData.CaptainCache[nation];

        // Copy the default captain object to trigger an update on the ComboBox selection.
        // Necessary because it only updates when the item itself is changed.
        return (ShallowCopyCaptain(defaultCaptain), nationCaptains);
    }

    private static Captain ShallowCopyCaptain(Captain original)
    {
        return new()
        {
            Id = original.Id,
            Index = original.Index,
            Name = original.Name,
            HasSpecialSkills = original.HasSpecialSkills,
            Skills = original.Skills,
            UniqueSkills = original.UniqueSkills,
            Nation = original.Nation,
        };
    }

    /// <summary>
    /// Add a skill to <see cref="SkillOrderList"/>.
    /// </summary>
    /// <param name="skill"> the <see cref="Skill"/> to add.</param>
    public void AddSkill(Skill skill)
    {
        if (this.SkillOrderList.Contains(skill))
        {
            this.logger.LogDebug("Adding skill {Skill} to skill order list", skill.SkillNumber);
            this.SkillOrderList.Remove(skill);
            this.ReorderSkillList();
            int pointCost = skill.Tiers.First(x => x.ShipClass == this.currentClass).Tier + 1;
            this.AssignedPoints -= pointCost;
            if (skill.SkillNumber == ArSkillNumber || skill.SkillNumber == ArSkillNumberSubs)
            {
                this.ShowArHpSelection = false;
            }

            if (skill.ConditionalModifierGroups is { Count: > 0 })
            {
                var skillName = this.SkillList!.Single(x => x.Value.Skill.Equals(skill)).Key;
                this.ConditionalModifiersList.RemoveRange(this.ConditionalModifiersList.Where(x => x.SkillName.Equals(skillName, StringComparison.Ordinal)));
            }

            this.RaisePropertyChanged(nameof(this.SkillOrderList));
        }
        else
        {
            this.logger.LogDebug("Removing skill {Skill} from skill order list", skill.SkillNumber);
            this.SkillOrderList.Add(skill);
            var pointCost = skill.Tiers.First(x => x.ShipClass == this.currentClass).Tier + 1;
            this.AssignedPoints += pointCost;
            if (skill.SkillNumber is ArSkillNumber or ArSkillNumberSubs)
            {
                this.ShowArHpSelection = true;
            }

            if (skill.ConditionalModifierGroups is { Count: > 0 })
            {
                this.ConditionalModifiersList.AddRange(this.CreateItemViewModelForSkill(skill));
            }

            this.RaisePropertyChanged(nameof(this.SkillOrderList));
        }

        this.SkillActivationButtonEnabled = this.CaptainTalentsList.Count > 0 || this.ConditionalModifiersList.Count > 0 || this.ShowArHpSelection;
    }

    /// <summary>
    /// Create a list of modifiers in a tuple format (name, value) from the currently selected skill.
    /// </summary>
    /// <returns>The List of modifiers of the currently selected skill.</returns>
    public List<Modifier> GetModifiersList()
    {
        var modifiers = this.SkillOrderList.ToList()
            .Where(skill => skill.Modifiers.Any() && skill.SkillNumber != ArSkillNumber && skill.SkillNumber != ArSkillNumberSubs && skill.SkillNumber != FuriousSkillNumber && skill.SkillNumber != ImprovedRepairPartyReadinessSkillNumber)
            .SelectMany(m => m.Modifiers)
            .ToList();

        // filter out modifiers that are class specific
        modifiers = modifiers.Where(x => !x.Name.Contains('_') || x.Name.Contains("_" + this.currentClass) || x.Name.Contains("invisible_"))
            .ToList();

        if (this.SkillOrderList.Any(skill => skill.SkillNumber == 14))
        {
            var affectedProp = new HashSet<string>();
            affectedProp.Add("SurvivabilityDataContainer.FireResistance");
            modifiers.Add(new ("", "fireResistanceEnabled", -1, null, null, Unit.None, affectedProp, DisplayValueProcessingKind.None, ValueProcessingKind.DirectAdd));
        }

        if (this.SkillOrderList.Any(skill => skill.SkillNumber == 22))
        {
            var affectedProp = new HashSet<string>();
            affectedProp.Add("ConsumableDataContainer.Interceptor");
            modifiers.Add(new("", "interceptorSelected", 0, null, null, Unit.None, affectedProp, DisplayValueProcessingKind.None, ValueProcessingKind.Multiplier));
        }

        if (this.ConditionalModifiersList.Count > 0)
        {
            modifiers.AddRange(this.CollectConditionalModifiers());
        }

        var arSkill = this.SkillOrderList.SingleOrDefault(skill => skill.SkillNumber is ArSkillNumber or ArSkillNumberSubs);
        if (arSkill is not null)
        {
            var arModifier = arSkill.Modifiers.First(x => x.Name.Equals("lastChanceReloadCoefficient"));
            var value = arModifier.Value * (100 - this.ArHpPercentage);
            modifiers.Add(new Modifier(arModifier.Name, value, arModifier.Location, arModifier));
        }

        if (this.CaptainTalentsList.Count > 0)
        {
            modifiers.AddRange(this.CollectTalentModifiers());
        }

        return modifiers;
    }

    /// <summary>
    /// Create a list of skill numbers from the currently selected list.
    /// </summary>
    /// <returns>The list of currently selected skill numbers.</returns>
    public List<int> GetSkillNumberList() => this.SkillOrderList.Select(skill => skill.SkillNumber).ToList();

    /// <summary>
    /// Return the index of the selected captain.
    /// </summary>
    /// <returns>The index of the selected captain.</returns>
    public string GetCaptainIndex()
    {
        return this.SelectedCaptain!.Index;
    }

    public void LoadBuild(IEnumerable<int> selectedSkills, string? captainIndex)
    {
        // this check is purely for backward compatibility
        if (captainIndex != null)
        {
            var captain = this.CaptainList!.Find(x => x.Index.Equals(captainIndex, StringComparison.Ordinal));
            this.SelectedCaptain = captain ?? this.CaptainList![0];
        }

        var skills = selectedSkills.Select(skillId => this.SelectedCaptain!.Skills.First(captainSkill => captainSkill.Value.SkillNumber == skillId)).Select(pair => pair.Value);
        this.SkillOrderList.AddRange(skills);
        this.AssignedPoints = this.SkillOrderList.Sum(skill => skill.Tiers.First(t => t.ShipClass == this.currentClass).Tier + 1);
        this.ReorderSkillList();
        foreach (var skill in this.SkillOrderList)
        {
            if (skill.SkillNumber is ArSkillNumber or ArSkillNumberSubs)
            {
                this.ShowArHpSelection = true;
            }

            if (skill.ConditionalModifierGroups.Any())
            {
                this.ConditionalModifiersList.AddRange(this.CreateItemViewModelForSkill(skill));
            }
        }

        this.SkillActivationButtonEnabled = this.CaptainTalentsList.Count > 0 || this.ConditionalModifiersList.Count > 0 || this.ShowArHpSelection;
    }

    private IEnumerable<Modifier> CollectConditionalModifiers()
    {
        var modifiers = new List<Modifier>();
        var conditionalModifiers = this.ConditionalModifiersList.Where(skill => skill.Status && skill.SkillId != FuriousSkillNumber && skill.MaximumActivations <= 1)
            .SelectMany(skill => skill.Modifiers);

        modifiers.AddRange(conditionalModifiers);

        // Custom handling for Furious skill. Needs to take into account the number of fires
        var furiousSkill = this.SkillOrderList.SingleOrDefault(skill => skill.SkillNumber is FuriousSkillNumber);
        var furiousSkillModifier = this.ConditionalModifiersList.SingleOrDefault(skill => skill.SkillId is FuriousSkillNumber);
        if (furiousSkill is not null && furiousSkillModifier is not null && furiousSkillModifier.Status)
        {
            var furiousModifiers = furiousSkill.ConditionalModifierGroups[0].Modifiers;
            var repeatableFirstModifier = furiousModifiers.First(x => x.Name.Equals("repeatable_first_GMShotDelay"));
            var multiplier = repeatableFirstModifier.Value;
            if (furiousSkillModifier.ActivationNumbers > 1)
            {
                multiplier *= float.Pow(furiousModifiers.First(x => x.Name.Equals("repeatable_other_GMShotDelay")).Value, furiousSkillModifier.ActivationNumbers - 1);
            }

            modifiers.Add(new("repeatableTotalGMShotDelay", multiplier, "", repeatableFirstModifier));
        }

        // Custom handling for Improved Repair Party Readiness Skill
        var irprModifierGroups = this.ConditionalModifiersList.Where(skill => skill.SkillId is ImprovedRepairPartyReadinessSkillNumber);
        foreach (var modifierGroup in irprModifierGroups.Where(vm => vm.Status && vm.MaximumActivations != 1))
        {
            var modifier = modifierGroup.Modifiers.First(x => x.Name.Equals("regenCrewReloadCoeff"));
            var skillFactor = modifier.Value;
            var multiplier = float.Pow(skillFactor, modifierGroup.ActivationNumbers);

            modifiers.Add(new(modifier.Name, multiplier, "", modifier));
        }

        return modifiers;
    }

    private IEnumerable<Modifier> CollectTalentModifiers()
    {
        var modifiers = new List<Modifier>();
        var talentModifiers = this.CaptainTalentsList.Where(talent => talent is { Status: true, MaximumActivations: <= 1 } && !talent.Modifiers.Any(modifier => modifier.Name.Equals("burnProbabilityBonus", StringComparison.Ordinal)))
            .SelectMany(skill => skill.Modifiers);
        modifiers.AddRange(talentModifiers);

        var talentMultipleActivationModifiers = this.CaptainTalentsList.Where(talent => talent is { Status: true, MaximumActivations: > 1 } && !talent.Modifiers.Any(modifier => modifier.Name.Equals("burnProbabilityBonus", StringComparison.Ordinal)))
            .SelectMany(talent => talent.Modifiers.Select(modifier => new Modifier(modifier.Name, float.Pow(modifier.Value, talent.ActivationNumbers), "", modifier)));
        modifiers.AddRange(talentMultipleActivationModifiers);

        var talentFireChanceModifier = this.CaptainTalentsList.Where(talent => talent.Status && talent.Modifiers.Any(modifier => modifier.Name.Equals("burnProbabilityBonus", StringComparison.Ordinal)))
            .SelectMany(talent => talent.Modifiers.Select(modifier => new Modifier(modifier.Name, float.Round(modifier.Value * talent.ActivationNumbers, 2), "", modifier)));
        modifiers.AddRange(talentFireChanceModifier);

        return modifiers;
    }

    private void CaptainTalentsListOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.TalentOrConditionalSkillEnabled = (this.showArHpSelection && this.ArHpPercentage < 100) || this.CaptainTalentsList.Any(talent => talent.Status);
    }

    private void ConditionalModifiersListOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.TalentOrConditionalSkillEnabled = (this.showArHpSelection && this.ArHpPercentage < 100) || this.ConditionalModifiersList.Any(skill => skill.Status);
    }

    /// <summary>
    /// Get a <see><cref>Dictionary{string, Skill}</cref></see> for the class indicated by <paramref name="shipClass"/> from <paramref name="captain"/>.
    /// </summary>
    /// <param name="shipClass"> The <see cref="ShipClass"/> for which to take the skills.</param>
    /// <param name="captain"> The <see cref="Captain"/> from which to take the skills.</param>
    /// <returns>A dictionary containing the skill for the class from the captain.</returns>
    private Dictionary<string, SkillItemViewModel> ConvertSkillToViewModel(ShipClass shipClass, Captain? captain)
    {
        this.logger.LogDebug("Getting skill for class {ShipClass} from captain {CaptainName}", shipClass.ToString(), captain!.Name);
        var skills = captain.Skills;

        var filteredSkills = skills.Where(x => x.Value.LearnableOn.Contains(shipClass)).ToList();

        var dictionary = filteredSkills.ToDictionary(x => x.Key, x => new SkillItemViewModel(x.Value, this, shipClass, this.canAddSkillCache, this.canRemoveSkillCache));
        this.logger.LogDebug("Found {SkillCount} skills", dictionary.Count);
        return dictionary;
    }

    /// <summary>
    /// Helper method to trigger a reevaluation of the <see cref="SkillItemViewModel.CanExecute"/> property of the skill view models.
    /// Also responsible for resetting the result cache after each evaluation.
    /// </summary>
    private void UpdateCanAddSkill()
    {
        if (this.SkillList == null)
        {
            return;
        }

        this.canAddSkillCache.Clear();
        this.canRemoveSkillCache.Clear();
        foreach (KeyValuePair<string, SkillItemViewModel> skill in this.SkillList)
        {
            skill.Value.CanExecuteChanged();
        }
    }

    private IEnumerable<SkillActivationItemViewModel> CreateItemViewModelForSkill(Skill skill)
    {
        var skillName = this.SkillList!.Single(x => x.Value.Skill.Equals(skill)).Key;
        if (skill.SkillNumber is FuriousSkillNumber)
        {
            yield return new(skillName, skill.SkillNumber, skill.ConditionalModifierGroups[0].Modifiers, false, skill.ConditionalModifierGroups[0].ActivationLimit);
        }
        else if (skill.SkillNumber is ImprovedRepairPartyReadinessSkillNumber)
        {
            yield return new(skillName, skill.SkillNumber, skill.ConditionalModifierGroups[0].Modifiers, false, skill.ConditionalModifierGroups[0].ActivationLimit);
            yield return new(skillName, skill.SkillNumber, skill.ConditionalModifierGroups[1].Modifiers, false, skill.ConditionalModifierGroups[1].ActivationLimit);
        }
        else
        {
            foreach (var modifierGroup in skill.ConditionalModifierGroups)
            {
                yield return new(skillName, skill.SkillNumber, modifierGroup.Modifiers, false);
            }
        }
    }

    /// <summary>
    /// Reorder the skills in <see cref="SkillOrderList"/> to make the skill order possible.
    /// It works with the assumption that if the first skill of a certain tier got removed, it should be replaced by the first skill of the same tier selected by the user.
    /// </summary>
    private void ReorderSkillList()
    {
        if (this.SkillOrderList.Count == 0)
        {
            return;
        }

        this.logger.LogDebug("Reordering skills");

        var groups = this.SkillOrderList.GroupBy(skill => skill.Tiers.First(x => x.ShipClass == this.currentClass).Tier)
            .Select(x => x.ToList())
            .ToList()
            .OrderBy(x => x[0].Tiers.First(skillPosition => skillPosition.ShipClass == this.currentClass).Tier)
            .ToList();

        // Tier 0 skill reordering
        this.ReorderTier0Skills(groups[0]);

        // Tier 1 skill reordering
        if (groups.Count > 2)
        {
            this.ReorderTierSkills(groups[1], 1);
        }

        // Tier 2 skill reordering
        if (groups.Count > 3)
        {
            this.ReorderTierSkills(groups[2], 2);
        }

        this.logger.LogDebug("Finished reordering skills");
    }

    private void ReorderTier0Skills(List<Skill> tier0Skills)
    {
        var tier0SkillsFirst = false;
        foreach (var tier0Skill in tier0Skills)
        {
            if (this.SkillOrderList.IndexOf(tier0Skill) == 0)
            {
                tier0SkillsFirst = true;
            }
        }

        if (!tier0SkillsFirst)
        {
            var firstTier0Skill = tier0Skills[0];
            this.SkillOrderList.Remove(firstTier0Skill);
            this.SkillOrderList.Insert(0, firstTier0Skill);
        }
    }

    private void ReorderTierSkills(List<Skill> tierSkills, int skillTier)
    {
        var firstPrevTierSkillIndex = this.SkillOrderList.FindIndex(skill => skill.Tiers.First(x => x.ShipClass == this.currentClass).Tier == skillTier - 1);
        var firstHigherTierSkillIndex = this.SkillOrderList.FindIndex(skill => skill.Tiers.First(x => x.ShipClass == this.currentClass).Tier > skillTier);

        var currentTierSkillFirst = false;

        foreach (var skill in tierSkills)
        {
            if (this.SkillOrderList.IndexOf(skill) > firstPrevTierSkillIndex && this.SkillOrderList.IndexOf(skill) < firstHigherTierSkillIndex)
            {
                currentTierSkillFirst = true;
            }
        }

        if (!currentTierSkillFirst)
        {
            var skill = tierSkills[0];
            this.SkillOrderList.Remove(skill);
            this.SkillOrderList.Insert(firstHigherTierSkillIndex, skill);
        }
    }
}
