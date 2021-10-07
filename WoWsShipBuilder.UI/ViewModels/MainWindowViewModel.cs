using System.Collections.Generic;
using System.Windows.Input;
using ReactiveUI;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private readonly MainWindow? self;

        private CaptainSkillSelectorViewModel? captainSkillSelectorViewModel;

        private ShipModuleViewModel shipModuleViewModel = null!;

        private ShipStatsControlViewModel? shipStatsControlViewModel;

        private SignalSelectorViewModel? signalSelectorViewModel;

        public MainWindowViewModel(MainWindow win)
        {
            self = win;

            // Signal selector model
            SignalSelectorViewModel = new SignalSelectorViewModel(0, AddSignalModifiers);

            // Ship stats model
            var ships = AppDataHelper.Instance.ReadLocalJsonData<Ship>(Nation.Germany, ServerType.Live);
            var shipData = ships!["PGSD109"];
            ShipStatsControlViewModel = new ShipStatsControlViewModel(shipData);

            // Captain Skill model
            CaptainSkillSelectorViewModel = new CaptainSkillSelectorViewModel(shipData.ShipClass);

            OpenSaveBuildCommand = ReactiveCommand.Create(() => OpenSaveBuild());
            BackToMenuCommand = ReactiveCommand.Create(() => BackToMenu());
            NewShipSelectionCommand = ReactiveCommand.Create(() => NewShipSelection());
            ShipModuleViewModel = new ShipModuleViewModel(shipData.ShipUpgradeInfo);
        }

        public MainWindowViewModel()
        {
            // Signal selector model
            SignalSelectorViewModel = new SignalSelectorViewModel(0, AddSignalModifiers);

            // Ship stats model
            ShipStatsControlViewModel = new ShipStatsControlViewModel(new Ship());

            // Captain Skill model
            CaptainSkillSelectorViewModel = new CaptainSkillSelectorViewModel(ShipClass.Destroyer);

            OpenSaveBuildCommand = ReactiveCommand.Create(() => OpenSaveBuild());
            BackToMenuCommand = ReactiveCommand.Create(() => BackToMenu());
            NewShipSelectionCommand = ReactiveCommand.Create(() => NewShipSelection());
            ShipModuleViewModel = new ShipModuleViewModel(); // TODO: replace with actual data
        }

        public ICommand OpenSaveBuildCommand { get; }

        public ICommand BackToMenuCommand { get; }

        public ICommand NewShipSelectionCommand { get; }

        public ShipModuleViewModel ShipModuleViewModel
        {
            get => shipModuleViewModel;
            set => this.RaiseAndSetIfChanged(ref shipModuleViewModel, value);
        }

        public SignalSelectorViewModel? SignalSelectorViewModel
        {
            get => signalSelectorViewModel;
            set => this.RaiseAndSetIfChanged(ref signalSelectorViewModel, value);
        }

        public ShipStatsControlViewModel? ShipStatsControlViewModel
        {
            get => shipStatsControlViewModel;
            set => this.RaiseAndSetIfChanged(ref shipStatsControlViewModel, value);
        }

        public CaptainSkillSelectorViewModel? CaptainSkillSelectorViewModel
        {
            get => captainSkillSelectorViewModel;
            set => this.RaiseAndSetIfChanged(ref captainSkillSelectorViewModel, value);
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

        private void OpenSaveBuild()
        {
            var win = new BuildCreationWindow();
            win.DataContext = new BuildCreationWindowViewModel(win, new Build());
            win.ShowDialog(self);
        }

        private void BackToMenu()
        {
            StartingMenuWindow win = new();
            StartMenuViewModel model = new(win);
            win.DataContext = model;
            win.Show();
            self!.Close();
        }

        private void NewShipSelection()
        {
            // Insert opening window
        }
    }
}
