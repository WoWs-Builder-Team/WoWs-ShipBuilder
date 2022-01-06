using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class SkillActivationItemViewModel : ViewModelBase
    {
        public SkillActivationItemViewModel(string name, int skillId, bool activationStatus)
        {
            SkillName = name;
            Status = activationStatus;
            SkillId = skillId;
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

        public int SkillId { get; set; }
    }
}
