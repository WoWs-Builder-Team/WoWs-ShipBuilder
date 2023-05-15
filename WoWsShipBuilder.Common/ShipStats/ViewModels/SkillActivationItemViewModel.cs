using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.Common.ShipStats.ViewModels;

public partial class SkillActivationItemViewModel : ViewModelBase
{
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

    [Observable]
    private string skillName = default!;

    [Observable]
    private bool status;

    [Observable]
    private int activationNumbers;

    public Dictionary<string, float> Modifiers { get; }

    public int SkillId { get; }

    public int MaximumActivations { get; }

    public string Description { get; }
}
