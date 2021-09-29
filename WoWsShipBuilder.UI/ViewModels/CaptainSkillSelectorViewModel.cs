using System.Collections.Generic;
using ReactiveUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    class CaptainSkillSelectorViewModel : ViewModelBase
    {
        private List<Skill>? skillList;

        public List<Skill>? SkillList
        {
            get => skillList;
            set => this.RaiseAndSetIfChanged(ref skillList, value);
        }
    }
}
