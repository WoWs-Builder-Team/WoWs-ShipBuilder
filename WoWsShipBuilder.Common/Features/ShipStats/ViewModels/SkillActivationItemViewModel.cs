using ReactiveUI;
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

    public SkillActivationItemViewModel(string name, int skillId, Dictionary<string, float> modifiers, bool activationStatus, int maximumActivations = 0, int activationNumbers = 1, string description = "")
    {
        SkillName = name;
        Status = activationStatus;
        SkillId = skillId;
        MaximumActivations = maximumActivations;
        ActivationNumbers = activationNumbers;
        Modifiers = modifiers;
        Description = description;
    }

    public Dictionary<string, float> Modifiers { get; }

    public int SkillId { get; }

    public int MaximumActivations { get; }

    public string Description { get; }
}
