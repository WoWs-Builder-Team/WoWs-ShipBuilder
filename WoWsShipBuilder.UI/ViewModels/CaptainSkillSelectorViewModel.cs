using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    class CaptainSkillSelectorViewModel : ViewModelBase
    {

        public CaptainSkillSelectorViewModel(ShipClass shipClass)
        {         
            SkillList = GetSkillsForClass(shipClass);
            AddSkillCommand = ReactiveCommand.Create<string>(x => AddSkill(x));
            SkillOrderList = new AvaloniaList<string>();
        }

        public ICommand AddSkillCommand { get; }

        private Dictionary<string, Skill>? skillList;

        public Dictionary<string, Skill>? SkillList
        {
            get => skillList;
            set => this.RaiseAndSetIfChanged(ref skillList, value);
        }
            
        private AvaloniaList<string> skillOrderList;

        public AvaloniaList<string> SkillOrderList
        {
            get => skillOrderList;
            set => this.RaiseAndSetIfChanged(ref skillOrderList, value);
        }

        private Dictionary<string,Skill> GetSkillsForClass(ShipClass shipClass, string captainId = "PCW001_CrewCommon")
        {
            var captain = AppDataHelper.Instance.ReadLocalJsonData<Captain>(null, ServerType.Live);
            var skills = captain![captainId].Skills;
            var filteredSkills = skills.Where(x => x.Value.LearnableOn.Contains(shipClass)).ToList();
            filteredSkills.ForEach(skill =>
            {
                var classSkill = skill.Value.Tiers.Where(x => x.ShipClass == shipClass).ToList();
                var first = classSkill.First();
                Debug.WriteLine(first.XPosition);
                skill.Value.Tiers = classSkill;
            });
            var filteredDictionary = filteredSkills.ToDictionary(x => x.Key, x => x.Value);
            return filteredDictionary;
        }

        private void AddSkill(string skillName)
        {
            if (SkillOrderList.Contains(skillName))
            {
                SkillOrderList.Remove(skillName);
                this.RaisePropertyChanged(nameof(SkillOrderList));
            }
            else
            {
                SkillOrderList.Add(skillName);
                this.RaisePropertyChanged(nameof(SkillOrderList));
            }
            
        }
    }
}
