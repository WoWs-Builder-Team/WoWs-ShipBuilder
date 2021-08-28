using System.Collections.Generic;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

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
                            VerticalPosition = -0.5f,
                            HorizontalSector = new[] { -146f, 146 },
                        },
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = 0.5f,
                            HorizontalSector = new[] { -150f, 150f },
                        },
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = 1.5f,
                            HorizontalSector = new[] { -150f, 150f },
                        },
                        new()
                        {
                            HorizontalPosition = 2,
                            VerticalPosition = 3.25f,
                            HorizontalSector = new[] { 0f, 166f },
                        },
                        new()
                        {
                            HorizontalPosition = 0,
                            VerticalPosition = 3.25f,
                            HorizontalSector = new[] { -166f, 0f },
                        },
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = 4.25f,
                            HorizontalSector = new[] { -150f, 150f },
                        },
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = 5.25f,
                            HorizontalSector = new[] { -148f, 148f },
                        },
                        new()
                        {
                            HorizontalPosition = 1,
                            VerticalPosition = 6.25f,
                            HorizontalSector = new[] { -150f, 150f },
                        },
                    },
                };
            }
        }
    }
}
