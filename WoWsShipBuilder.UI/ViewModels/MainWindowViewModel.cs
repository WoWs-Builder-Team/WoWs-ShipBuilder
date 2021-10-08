using System;
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

        private string baseXp = "0";

        public string BaseXp
        {
            get => baseXp;
            set
            {
                this.RaiseAndSetIfChanged(ref baseXp, value);
                CalculateXPValues();
            }
        }

        private string xpBonus = "0";

        public string XpBonus
        {
            get => xpBonus;
            set
            {
                this.RaiseAndSetIfChanged(ref xpBonus, value);
                CalculateXPValues();
            }
        }

        private string commanderXpBonus = "0";

        public string CommanderXpBonus
        {
            get => commanderXpBonus;
            set
            {
                this.RaiseAndSetIfChanged(ref commanderXpBonus, value);
                CalculateXPValues();
            }
        }

        private string freeXpBonus = "0";

        public string FreeXpBonus
        {
            get => freeXpBonus;
            set
            {
                this.RaiseAndSetIfChanged(ref freeXpBonus, value);
                CalculateXPValues();
            }
        }

        private string xp = "0";

        public string Xp
        {
            get => xp;
            set => this.RaiseAndSetIfChanged(ref xp, value);
        }

        private string commanderXp = "0";

        public string CommanderXp
        {
            get => commanderXp;
            set => this.RaiseAndSetIfChanged(ref commanderXp, value);
        }

        private string freeXp = "0";

        public string FreeXp
        {
            get => freeXp;
            set => this.RaiseAndSetIfChanged(ref freeXp, value);
        }

        private string accountType = "Normal Account";

        public string AccountType
        {
            get => accountType;
            set
            {
                this.RaiseAndSetIfChanged(ref accountType, value);
                CalculateXPValues();
            }
        }


        private bool? accountState = false;

        public bool? AccountState
        {
            get => accountState;
            set
            {
                this.RaiseAndSetIfChanged(ref accountState, value);
                ChangeAccountType(value);
            }
        }

        public UpgradePanelViewModel UpgradePanelViewModel { get; } = new();

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

        private void CalculateXPValues()
        {
            if (!string.IsNullOrEmpty(BaseXp) && !string.IsNullOrEmpty(XpBonus) && !string.IsNullOrEmpty(CommanderXpBonus) && !string.IsNullOrEmpty(FreeXpBonus))
            {
                var baseXp = Convert.ToInt32(BaseXp);
                var xpBonus = Convert.ToDouble(XpBonus);
                var commanderXpBonus = Convert.ToDouble(CommanderXpBonus);
                var freeXpBonus = Convert.ToDouble(FreeXpBonus);

                double accountMultiplier = 1;
                if (AccountType.Equals("WG Premium Account"))
                {
                    accountMultiplier = 1.5;
                }
                else if (AccountType.Equals("WoWs Premium Account"))
                {
                    accountMultiplier = 1.65;
                }

                // The 1 represent the account type modifier. For now, the math is done only for non premium account
                int finalXp = (int)(baseXp * accountMultiplier * (1 + (xpBonus / 100)));
                int commanderXp = (int)(finalXp + (baseXp * accountMultiplier * (commanderXpBonus / 100)));
                int freeXp = (int)(finalXp * 0.05 * (1 + (freeXpBonus / 100)));

                Xp = Convert.ToString(finalXp);
                CommanderXp = Convert.ToString(commanderXp);
                FreeXp = Convert.ToString(freeXp);
            }
        }

        private void ChangeAccountType(bool? accountState)
        {
            if (accountState.HasValue)
            {
                if (accountState.Value)
                {
                    AccountType = "WoWs Premium Account";
                }
                else
                {
                    AccountType = "Normal Account";
                }
            }
            else
            {
                AccountType = "WG Premium Account";
            }
        }
    }
}
