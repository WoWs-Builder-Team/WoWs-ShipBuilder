using System.Collections.Generic;
using ReactiveUI;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class SkillActivationItemViewModel : ViewModelBase
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

        private string skillName = default!;

        public string SkillName
        {
            get => skillName;
            set => this.RaiseAndSetIfChanged(ref skillName, value);
        }

        private bool status;

        public bool Status
        {
            get => status;
            set => this.RaiseAndSetIfChanged(ref status, value);
        }

        private int activationNumbers;

        public int ActivationNumbers
        {
            get => activationNumbers;
            set => this.RaiseAndSetIfChanged(ref activationNumbers, value);
        }

        public Dictionary<string, float> Modifiers { get; } = new();

        public int SkillId { get; }

        public int MaximumActivations { get; }

        public string Description { get; }
    }
}
