using System.Collections.Immutable;
using FluentAssertions;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.Features.ShipStats.ViewModels;

namespace WoWsShipBuilder.Test.Features.ShipStats.CaptainSkillSelectorViewModelTests;

/// <summary>
/// Covers the talent handling introduced for game update 15.7: battle groups and tiered talents.
/// </summary>
[TestFixture]
public class Talents
{
    private const string RegularId = "TALENT_TEST_1_1";

    private const string EveryId = "TALENT_TEST_2_2";

    [Test]
    public void SelectedCaptain_TalentPairedPerBattleGroup_ListsItOnce()
    {
        var vm = CreateViewModel(
            Talent(RegularId, TalentBattleGroup.Regular),
            Talent(RegularId, TalentBattleGroup.Operations),
            Talent(EveryId, TalentBattleGroup.Every));

        vm.CaptainTalentsList.Select(talent => talent.SkillName).Should().BeEquivalentTo(RegularId, EveryId);
    }

    [Test]
    public void SelectedCaptain_Talent_UsesTheBattleGroupDescriptionKey()
    {
        var vm = CreateViewModel(Talent(RegularId, TalentBattleGroup.Regular));

        vm.CaptainTalentsList.Single().Description.Should().Be($"{RegularId}_DESCRIPTION_BATTLE_GROUP_REGULAR");
    }

    /// <summary>
    /// A captain whose talents are all operations-only leaves an empty list, so the flag has to follow the filtered
    /// list rather than the raw data or the app offers a talent panel with nothing in it.
    /// </summary>
    [Test]
    public void SelectedCaptain_OnlyOperationsTalents_ReportsNoTalents()
    {
        var vm = CreateViewModel(Talent(RegularId, TalentBattleGroup.Operations));

        vm.CaptainTalentsList.Should().BeEmpty();
        vm.CaptainWithTalents.Should().BeFalse();
    }

    [Test]
    public void SelectedCaptain_TieredTalent_ExposesOneTierPerLevel()
    {
        var vm = CreateViewModel(TieredTalent());

        var talent = vm.CaptainTalentsList.Single();

        talent.IsTiered.Should().BeTrue();
        talent.Tiers.Select(tier => tier.Level).Should().Equal(1, 2, 3);
        talent.MaximumActivations.Should().Be(3);
    }

    [Test]
    public void SelectedCaptain_TieredTalent_ActiveTierFollowsActivationCount()
    {
        var vm = CreateViewModel(TieredTalent());
        var talent = vm.CaptainTalentsList.Single();

        talent.ActivationNumbers = 1;
        talent.EffectiveModifiers.Single().Value.Should().BeApproximately(0.95f, 0.0001f);

        talent.ActivationNumbers = 2;
        talent.EffectiveModifiers.Single().Value.Should().BeApproximately(0.75f, 0.0001f);

        talent.ActivationNumbers = 3;
        talent.EffectiveModifiers.Single().Value.Should().BeApproximately(0.6f, 0.0001f);
    }

    /// <summary>
    /// Regression guard. Repeatable non-tiered talents are compounded with float.Pow over the activation count.
    /// A tier already reports its cumulative value, so routing it through that path would cube it: 0.6 would become
    /// 0.216 with no error anywhere.
    /// </summary>
    [Test]
    public void GetModifiersList_TieredTalent_UsesTheTierValueWithoutCompounding()
    {
        var vm = CreateViewModel(TieredTalent());
        var talent = vm.CaptainTalentsList.Single();
        talent.Status = true;
        talent.ActivationNumbers = 3;

        var modifiers = vm.GetModifiersList().Where(modifier => modifier.Name.Equals("GMShotDelay", StringComparison.Ordinal)).ToList();

        modifiers.Should().ContainSingle();
        modifiers.Single().Value.Should().BeApproximately(0.6f, 0.0001f);
    }

    /// <summary>
    /// A talent can mix escalating and non-escalating effects. Reading a tier must still give the whole talent, or
    /// the non-escalating stats disappear the moment any effect gains levels.
    /// </summary>
    [Test]
    public void SelectedCaptain_TieredTalentWithAConstantEffect_KeepsTheConstantStat()
    {
        var vm = CreateViewModel(MixedTalent());

        var talent = vm.CaptainTalentsList.Single();
        talent.ActivationNumbers = 2;

        talent.EffectiveModifiers.Select(modifier => modifier.Name).Should().BeEquivalentTo("GMShotDelay", "GSAlphaFactor");
    }

    /// <summary>
    /// The ladder can be shorter than the talent's cap, in which case the top tier persists for later activations.
    /// Taking the ladder length as the cap would stop the user below what the game allows.
    /// </summary>
    [Test]
    public void SelectedCaptain_LadderShorterThanTriggerCap_KeepsTheTalentCap()
    {
        var vm = CreateViewModel(MixedTalent());

        var talent = vm.CaptainTalentsList.Single();

        talent.MaximumActivations.Should().Be(4);
        talent.Tiers.Should().HaveCount(2);

        talent.ActivationNumbers = 4;
        talent.ActiveTier!.Level.Should().Be(2, "the top tier persists past the end of the ladder");
    }

    /// <summary>
    /// Before the ladder starts, falling through to the flat modifier list would report the fully escalated values,
    /// which are stronger than any tier actually reached.
    /// </summary>
    [Test]
    public void SelectedCaptain_TieredTalentBelowTheFirstTier_DoesNotReportFullStrengthValues()
    {
        var vm = CreateViewModel(TieredTalent());

        var talent = vm.CaptainTalentsList.Single();
        talent.ActivationNumbers = 0;

        talent.ActiveTier!.Level.Should().Be(1);
        talent.EffectiveModifiers.Single().Value.Should().BeApproximately(0.95f, 0.0001f);
    }

    /// <summary>
    /// Two escalating effects can have ladders of different lengths. The shorter one must hold at its top value for
    /// the remaining tiers rather than disappearing from them.
    /// </summary>
    [Test]
    public void SelectedCaptain_EffectsWithUnevenLadders_HoldTheShorterOneAtItsTopTier()
    {
        var vm = CreateViewModel(UnevenLadderTalent());

        var talent = vm.CaptainTalentsList.Single();

        talent.Tiers.Should().HaveCount(3);
        talent.Tiers[2].Modifiers.Select(modifier => modifier.Name).Should().BeEquivalentTo("GMShotDelay", "GTShotDelay");
        talent.Tiers[2].Modifiers.Single(modifier => modifier.Name == "GTShotDelay").Value.Should().BeApproximately(0.8f, 0.0001f);
    }

    /// <summary>
    /// The activation threshold lives on the trigger's activator, not on the effect, and which entry holds it
    /// depends on the activator type.
    /// </summary>
    [Test]
    public void SelectedCaptain_TieredTalent_CarriesThePerTierActivationThreshold()
    {
        var vm = CreateViewModel(TalentWithTrigger("DamageDoneActivator", ("damageIncrement", 30000m), ("damageIncrement", 60000m)));

        var talent = vm.CaptainTalentsList.Single();

        talent.Tiers.Select(tier => tier.ActivationThreshold).Should().Equal(30000m, 60000m);
    }

    /// <summary>
    /// Activators list several threshold entries, most of them zero. The chosen one has to be stable, because the
    /// threshold collection is hash-ordered.
    /// </summary>
    [Test]
    public void SelectedCaptain_ActivatorWithZeroedCompanionThresholds_PicksTheMeaningfulOne()
    {
        var trigger = new TalentTrigger(
            "DamageDoneActivator",
            ImmutableDictionary<string, decimal>.Empty,
            -1,
            ImmutableList.Create(
                new TalentTriggerLevel(1, new Dictionary<string, decimal>
                {
                    ["damageIncrementPerTotalEnemyTeamHealth"] = 0m,
                    ["damageIncrement"] = 30000m,
                }.ToImmutableDictionary())));

        var vm = CreateViewModel(TieredTalentWith(trigger, 1));

        vm.CaptainTalentsList.Single().Tiers.Single().ActivationThreshold.Should().Be(30000m);
    }

    [Test]
    public void SelectedCaptain_TalentWithoutTrigger_HasNoThreshold()
    {
        var vm = CreateViewModel(TieredTalent());

        vm.CaptainTalentsList.Single().Tiers.Should().OnlyContain(tier => tier.ActivationThreshold == null);
    }

    private static UniqueSkill TalentWithTrigger(string activatorType, params (string Key, decimal Value)[] perTier)
    {
        var levels = perTier
            .Select((entry, index) => new TalentTriggerLevel(index + 1, new Dictionary<string, decimal> { [entry.Key] = entry.Value }.ToImmutableDictionary()))
            .ToImmutableList();

        var trigger = new TalentTrigger(activatorType, ImmutableDictionary<string, decimal>.Empty, -1, levels);
        return TieredTalentWith(trigger, perTier.Length);
    }

    private static UniqueSkill TieredTalentWith(TalentTrigger trigger, int tierCount)
    {
        var levels = Enumerable.Range(1, tierCount)
            .Select(level => Level(level, "speedCoef", 1f + (level * 0.01f)))
            .ToImmutableList();

        return new()
        {
            TranslationId = RegularId,
            BattleGroup = TalentBattleGroup.Regular,
            MaxTriggerNum = tierCount,
            Trigger = trigger,
            SkillEffects = ImmutableDictionary<string, UniqueSkillEffect>.Empty.Add(
                "Effect",
                new(false, 1, ImmutableList.Create(Modifier("speedCoef", 1.05f))) { Levels = levels }),
        };
    }

    private static UniqueSkill UnevenLadderTalent() => new()
    {
        TranslationId = RegularId,
        BattleGroup = TalentBattleGroup.Regular,
        MaxTriggerNum = 3,
        SkillEffects = ImmutableDictionary<string, UniqueSkillEffect>.Empty
            .Add("Long", new(false, 1, ImmutableList.Create(Modifier("GMShotDelay", 0.6f)))
            {
                Levels = ImmutableList.Create(
                    Level(1, "GMShotDelay", 0.9f),
                    Level(2, "GMShotDelay", 0.75f),
                    Level(3, "GMShotDelay", 0.6f)),
            })
            .Add("Short", new(false, 2, ImmutableList.Create(Modifier("GTShotDelay", 0.8f)))
            {
                Levels = ImmutableList.Create(
                    Level(1, "GTShotDelay", 0.9f),
                    Level(2, "GTShotDelay", 0.8f)),
            }),
    };

    private static UniqueSkillEffectLevel Level(int level, string name, float cumulative)
    {
        return new(level, ImmutableList.Create(Modifier(name, cumulative)), ImmutableList.Create(Modifier(name, cumulative)));
    }

    private static UniqueSkill MixedTalent() => new()
    {
        TranslationId = RegularId,
        BattleGroup = TalentBattleGroup.Regular,
        MaxTriggerNum = 4,
        SkillEffects = ImmutableDictionary<string, UniqueSkillEffect>.Empty
            .Add("Tiered", new(false, 1, ImmutableList.Create(Modifier("GMShotDelay", 0.8f)))
            {
                Levels = ImmutableList.Create(
                    new UniqueSkillEffectLevel(1, ImmutableList.Create(Modifier("GMShotDelay", 0.9f)), ImmutableList.Create(Modifier("GMShotDelay", 0.9f))),
                    new UniqueSkillEffectLevel(2, ImmutableList.Create(Modifier("GMShotDelay", 0.89f)), ImmutableList.Create(Modifier("GMShotDelay", 0.8f)))),
            })
            .Add("Constant", new(false, 2, ImmutableList.Create(Modifier("GSAlphaFactor", 1.1f)))),
    };

    private static CaptainSkillSelectorViewModel CreateViewModel(params UniqueSkill[] talents)
    {
        var captain = new Captain
        {
            Id = 1,
            Index = "PTW001",
            Name = "PTW001_Test",
            Nation = Nation.Usa,
            Skills = ImmutableDictionary<string, Skill>.Empty,
            UniqueSkills = talents.Select((talent, index) => (Key: $"Talent{index}", Value: talent))
                .ToImmutableDictionary(entry => entry.Key, entry => entry.Value),
        };

        return new(ShipClass.Cruiser, (captain, null));
    }

    private static UniqueSkill Talent(string translationId, TalentBattleGroup battleGroup) => new()
    {
        TranslationId = translationId,
        BattleGroup = battleGroup,
        MaxTriggerNum = 1,
        SkillEffects = ImmutableDictionary<string, UniqueSkillEffect>.Empty.Add(
            "Effect",
            new(false, 1, ImmutableList.Create(Modifier("GMShotDelay", 0.9f)))),
    };

    private static UniqueSkill TieredTalent() => new()
    {
        TranslationId = RegularId,
        BattleGroup = TalentBattleGroup.Regular,
        MaxTriggerNum = 3,
        SkillEffects = ImmutableDictionary<string, UniqueSkillEffect>.Empty.Add(
            "Effect",
            new(false, 1, ImmutableList.Create(Modifier("GMShotDelay", 0.6f)))
            {
                // Increments and cumulative values as the game ships them: 0.95, then 0.95 x 0.78947 = 0.75,
                // then 0.75 x 0.8 = 0.6.
                Levels = ImmutableList.Create(
                    new UniqueSkillEffectLevel(1, ImmutableList.Create(Modifier("GMShotDelay", 0.95f)), ImmutableList.Create(Modifier("GMShotDelay", 0.95f))),
                    new UniqueSkillEffectLevel(2, ImmutableList.Create(Modifier("GMShotDelay", 0.78947f)), ImmutableList.Create(Modifier("GMShotDelay", 0.75f))),
                    new UniqueSkillEffectLevel(3, ImmutableList.Create(Modifier("GMShotDelay", 0.8f)), ImmutableList.Create(Modifier("GMShotDelay", 0.6f)))),
            }),
    };

    private static Modifier Modifier(string name, float value) => new(name, value, "Test", null);
}
