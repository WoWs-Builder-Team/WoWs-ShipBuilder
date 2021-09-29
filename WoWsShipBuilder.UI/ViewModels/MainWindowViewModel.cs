using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ReactiveUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        #region Private internal fields
        #endregion

        public MainWindowViewModel()
        {
            var addSignalFlagModifiersCommand = ReactiveCommand.Create<string>(x => AddSignalModifiers(x));
            Action<string> action = x => AddSignalModifiers(x);
            SignalSelectorViewModel = new SignalSelectorViewModel(0, action);          
        }

        private SignalSelectorViewModel? signalSelectorViewModel;

        public SignalSelectorViewModel? SignalSelectorViewModel
        {
            get => signalSelectorViewModel;
            set => this.RaiseAndSetIfChanged(ref signalSelectorViewModel, value);
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

        private void AddSignalModifiers(string flagIndex)
        {
            if (SignalSelectorViewModel!.SelectedSignalIndex.Contains(flagIndex))
            {
                SignalSelectorViewModel.SelectedSignalIndex.Remove(flagIndex);
                SignalSelectorViewModel.SignalsNumber--;
            }
            else
            {
                SignalSelectorViewModel.SelectedSignalIndex.Add(flagIndex);
                SignalSelectorViewModel.SignalsNumber++;
            }
        }
    }
}
