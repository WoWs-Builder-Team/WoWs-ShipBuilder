using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool showAllAngles;
        private bool permanentAngleText;

        public MainWindowViewModel()
        {
            ToggleCommand = ReactiveCommand.Create(() => ShowAllAngles = !ShowAllAngles);
            ToggleTextCommand = ReactiveCommand.Create(() => PermanentAngleText = !PermanentAngleText);
        }

        public string Greeting => "Welcome to Avalonia!";

        public ICommand ToggleCommand { get; }

        public ICommand ToggleTextCommand { get; }

        public bool ShowAllAngles
        {
            get => showAllAngles;
            set => this.RaiseAndSetIfChanged(ref showAllAngles, value);
        }

        public bool PermanentAngleText
        {
            get => permanentAngleText;
            set => this.RaiseAndSetIfChanged(ref permanentAngleText, value);
        }

        public TurretModule TurretModule
        {
            get
            {
                return new TurretModule
                {
                    Guns = new List<Gun>
                    {
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = -0.5,
                            HorizontalSector = new double[] { -146, 146 },
                        },
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = 0.5,
                            HorizontalSector = new double[] { -150, 150 },
                        },
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = 1.5,
                            HorizontalSector = new double[] { -150, 150 },
                        },
                        new()
                        {
                            HorizontalPosition = 2,
                            VerticalPosition = 3.25,
                            HorizontalSector = new double[] { 0, 166 },
                        },
                        new()
                        {
                            HorizontalPosition = 0,
                            VerticalPosition = 3.25,
                            HorizontalSector = new double[] { -166, 0 },
                        },
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = 4.25,
                            HorizontalSector = new double[] { -150, 150 },
                        },
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = 5.25,
                            HorizontalSector = new double[] { -148, 148 },
                            HorizontalDeadZones = new[] { new[] { -20.0, 20.0 } },
                        },
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = 6.25,
                            HorizontalSector = new double[] { -150, 150 },
                        },
                    },
                };
            }
        }
    }
}
