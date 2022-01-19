using System.Collections.Generic;
using ReactiveUI;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Translations;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class FiringAngleViewModel : ViewModelBase
    {
        public FiringAngleViewModel(TurretModule turrets)
        {
            Turrets = turrets;
        }

        public FiringAngleViewModel()
        {
            var testData = DataHelper.LoadPreviewShip(ShipClass.Battleship, 10, Nation.Germany);
            var currentShipStats = ShipUI.FromShip(testData.Ship, testData.Configuration, new List<(string, float)>());
            Turrets = currentShipStats.MainBatteryUI!.OriginalMainBatteryData;
        }

        private TurretModule turret = null!;

        public TurretModule Turrets
        {
            get => turret;
            set => this.RaiseAndSetIfChanged(ref turret, value);
        }

        private bool showAllText = false;

        public bool ShowAllText
        {
            get => showAllText;
            set => this.RaiseAndSetIfChanged(ref showAllText, value);
        }

        private string showAllTextButton = Translation.FiringAngleWindow_ShowAll;

        public string ShowAllTextButton
        {
            get => showAllTextButton;
            set => this.RaiseAndSetIfChanged(ref showAllTextButton, value);
        }

        private bool permaText = true;

        public bool PermaText
        {
            get => permaText;
            set => this.RaiseAndSetIfChanged(ref permaText, value);
        }

        private string permaTextButton = Translation.FiringAngleWindow_PermaTextOff;

        public string PermaTextButton
        {
            get => permaTextButton;
            set => this.RaiseAndSetIfChanged(ref permaTextButton, value);
        }

        public void SetShowAll()
        {
            if (ShowAllText)
            {
                ShowAllText = false;
                ShowAllTextButton = Translation.FiringAngleWindow_ShowAll;
            }
            else
            {
                ShowAllText = true;
                ShowAllTextButton = Translation.FiringAngleWindow_HideAll;
            }
        }

        public void SetPermaText()
        {
            if (PermaText)
            {
                PermaText = false;
                PermaTextButton = Translation.FiringAngleWindow_PermaTextOn;
            }
            else
            {
                PermaText = true;
                PermaTextButton = Translation.FiringAngleWindow_PermaTextOff;
            }
        }
    }
}
