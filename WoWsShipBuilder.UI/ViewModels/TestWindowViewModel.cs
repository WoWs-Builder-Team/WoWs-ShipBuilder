using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class TestWindowViewModel : ViewModelBase
    {
        public TestWindowViewModel()
        {
            var ship = DataHelper.LoadPreviewShip(ShipClass.Destroyer, 10, Nation.Japan).Ship;
            var shellName = ship.MainBatteryModuleList.First().Value.Guns.First().AmmoList.Last();
            var shell = AppDataHelper.Instance.GetProjectile<ArtilleryShell>(shellName);
            var dispersionData = ship.MainBatteryModuleList.First().Value.DispersionValues;
            var sigma = ship.MainBatteryModuleList.First().Value.Sigma;
            var maxRange = ship.MainBatteryModuleList.First().Value.MaxRange;
            var aimingRange = 15000;
            Shots = 9;

            DispersionPlotParameters = DispersionPlotHelper.CalculateDispersionPlotParameters(dispersionData, shell, (double)maxRange, aimingRange, (double)sigma, Shots);
        }

        private DispersionEllipse dispersionPlotParameters = default!;

        public DispersionEllipse DispersionPlotParameters
        {
            get => dispersionPlotParameters;
            set => this.RaiseAndSetIfChanged(ref dispersionPlotParameters, value);
        }

        private int shots;

        public int Shots
        {
            get => shots;
            set => this.RaiseAndSetIfChanged(ref shots, value);
        }

        public void Apply()
        {
            var ship = DataHelper.LoadPreviewShip(ShipClass.Battleship, 10, Nation.Japan).Ship;
            var shellName = ship.MainBatteryModuleList.First().Value.Guns.First().AmmoList.Last();
            var shell = AppDataHelper.Instance.GetProjectile<ArtilleryShell>(shellName);
            var dispersionData = ship.MainBatteryModuleList.First().Value.DispersionValues;
            var sigma = ship.MainBatteryModuleList.First().Value.Sigma;
            var maxRange = ship.MainBatteryModuleList.First().Value.MaxRange;
            var aimingRange = 15000;
            DispersionPlotParameters = DispersionPlotHelper.CalculateDispersionPlotParameters(dispersionData, shell, (double)maxRange, aimingRange, (double)sigma, Shots);
        }
    }
}
