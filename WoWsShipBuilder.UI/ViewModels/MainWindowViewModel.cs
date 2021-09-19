using System.Collections.Generic;
using System.Windows.Input;
using ReactiveUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private int selectedModernization;

        public List<Modernization> TestModernizationList => new()
        {
            DataHelper.PlaceholderModernization,
            new Modernization
            {
                Name = "PCM001_MainGun_Mod_I",
                Index = "PCM001",
            },
            new Modernization
            {
                Name = "PCM002_Torpedo_Mod_I",
                Index = "PCM002",
            },
        };

        public ICommand ModernizationSelectedCommand { get; }

        public int SelectedModernization
        {
            get => selectedModernization;
            set => this.RaiseAndSetIfChanged(ref selectedModernization, value);
        }

        public MainWindowViewModel()
        {
            ModernizationSelectedCommand = ReactiveCommand.Create<Modernization>(obj =>
            {
                var index = TestModernizationList.FindIndex(modernization => modernization.Index == obj.Index);
                SelectedModernization = index;
            });
        }
    }
}
