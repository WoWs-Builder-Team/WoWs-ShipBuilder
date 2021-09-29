using ReactiveUI;
using System.Collections.Generic;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            FlagNumber = 5;
        }

        public List<Modernization> Slot1ModernizationList => new()
        {
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
            new Modernization
            {
                Name = "PCM004_AirDefense_Mod_I",
                Index = "PCM004",
            },
        };

        public List<Modernization> Slot2ModernizationList => new()
        {
            new Modernization
            {
                Name = "PCM002_Torpedo_Mod_I",
                Index = "PCM002",
            },
            new Modernization
            {
                Name = "PCM004_AirDefense_Mod_I",
                Index = "PCM004",
            },
        };

        public List<Modernization> Slot3ModernizationList => new()
        {
            new Modernization
            {
                Name = "PCM001_MainGun_Mod_I",
                Index = "PCM001",
            },
            new Modernization
            {
                Name = "PCM004_AirDefense_Mod_I",
                Index = "PCM004",
            },
            new Modernization
            {
                Name = "PCM012_SecondaryGun_Mod_II",
                Index = "PCM012",
            },
        };

        public List<Modernization> Slot4ModernizationList => new()
        {
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


        private int flagNumber;

        public int FlagNumber
        {
            get => flagNumber;
            set => this.RaiseAndSetIfChanged(ref flagNumber, value);
        }
    }
}
