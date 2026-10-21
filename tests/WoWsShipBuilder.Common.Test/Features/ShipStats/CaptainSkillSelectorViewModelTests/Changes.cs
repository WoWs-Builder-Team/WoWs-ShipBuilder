using System.Collections.Immutable;
using FluentAssertions;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.Features.ShipStats.ViewModels;

namespace WoWsShipBuilder.Test.Features.ShipStats.CaptainSkillSelectorViewModelTests;

/// <summary>
/// The talent and conditional skill popup deliberately only reports a change once it is closed again, so the ship
/// stats are not recalculated for every intermediate state while the user is still configuring the popup.
/// This rule lives in the viewmodel itself, so that the viewmodels owning it do not need to know about the popup.
/// </summary>
[TestFixture]
public class Changes
{
    [Test]
    public void Changes_SkillActivationPopupOpened_DoesNotEmit()
    {
        var vm = CreateViewModel();
        int emissions = 0;
        using var subscription = vm.Changes.Subscribe(_ => emissions++);
        int afterSubscribe = emissions;

        vm.SkillActivationPopupOpen = true;

        emissions.Should().Be(afterSubscribe);
    }

    [Test]
    public void Changes_SkillActivationPopupClosed_Emits()
    {
        var vm = CreateViewModel();
        vm.SkillActivationPopupOpen = true;
        int emissions = 0;
        using var subscription = vm.Changes.Subscribe(_ => emissions++);
        int afterSubscribe = emissions;

        vm.SkillActivationPopupOpen = false;

        emissions.Should().Be(afterSubscribe + 1);
    }

    [Test]
    public void IsModified_NoSkillSelected_ReturnsFalse()
    {
        CreateViewModel().IsModified.Should().BeFalse();
    }

    private static CaptainSkillSelectorViewModel CreateViewModel()
    {
        var captain = new Captain
        {
            Id = 1,
            Index = "PTW001",
            Name = "PTW001_Test",
            Nation = Nation.Usa,
            Skills = ImmutableDictionary<string, Skill>.Empty,
            UniqueSkills = ImmutableDictionary<string, UniqueSkill>.Empty,
        };

        return new(ShipClass.Cruiser, (captain, null));
    }
}
