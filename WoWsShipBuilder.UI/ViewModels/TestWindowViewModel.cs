using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class TestWindowViewModel : ViewModelBase
    {
        public TestWindowViewModel()
        {
            var ship = DataHelper.LoadPreviewShip(ShipClass.Battleship, 10, Nation.Japan).Ship;
            DispersionData = ship.MainBatteryModuleList.First().Value.DispersionValues;
            Sigma = ship.MainBatteryModuleList.First().Value.Sigma;
            MaxRange = ship.MainBatteryModuleList.First().Value.MaxRange;
            AimingRange = 15000;
            var shellName = ship.MainBatteryModuleList.First().Value.Guns.First().AmmoList.Last();
            Shell = AppDataHelper.Instance.GetProjectile<ArtilleryShell>(shellName);
        }

        private Dispersion dispersionData;

        public Dispersion DispersionData
        {
            get => dispersionData;
            set => this.RaiseAndSetIfChanged(ref dispersionData, value);
        }

        private decimal sigma;

        public decimal Sigma
        {
            get => sigma;
            set => this.RaiseAndSetIfChanged(ref sigma, value);
        }

        private decimal maxRange;

        public decimal MaxRange
        {
            get => maxRange;
            set => this.RaiseAndSetIfChanged(ref maxRange, value);
        }

        private decimal shotsNumber;

        public decimal ShotsNumber
        {
            get => shotsNumber;
            set => this.RaiseAndSetIfChanged(ref shotsNumber, value);
        }

        private decimal aimingRange;

        public decimal AimingRange
        {
            get => aimingRange;
            set => this.RaiseAndSetIfChanged(ref aimingRange, value);
        }

        private ArtilleryShell shell;

        public ArtilleryShell Shell
        {
            get => shell;
            set => this.RaiseAndSetIfChanged(ref shell, value);
        }
    }
}
