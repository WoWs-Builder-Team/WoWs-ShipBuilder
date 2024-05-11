using System.Collections.Immutable;
using ReactiveUI;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public partial class SkillActivationItemViewModel : ReactiveObject
{
    [Observable]
    private int activationNumbers;

    [Observable]
    private string skillName = default!;

    [Observable]
    private bool status;

    public SkillActivationItemViewModel(string name, int skillId, ImmutableList<Modifier> modifiers, bool activationStatus, int maximumActivations = 0, int activationNumbers = 1, string description = "")
    {
        this.SkillName = name;
        this.Status = activationStatus;
        this.SkillId = skillId;
        this.MaximumActivations = maximumActivations == -1 ? 99 : maximumActivations;
        this.ActivationNumbers = activationNumbers;
        this.Modifiers = modifiers;
        this.Description = description;
    }

    public ImmutableList<Modifier> Modifiers { get; }

    public int SkillId { get; }

    public int MaximumActivations { get; }

    public string Description { get; }
}
