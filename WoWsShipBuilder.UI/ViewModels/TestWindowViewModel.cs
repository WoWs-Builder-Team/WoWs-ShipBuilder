using ReactiveUI;
using WoWsShipBuilder.Core.DataUI;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class TestWindowViewModel : ViewModelBase
    {
        public TestWindowViewModel()
        {
            // var ship = DataHelper.LoadPreviewShip(ShipClass.Battleship, 10, Nation.Japan).Ship;
            // var shellName = ship.MainBatteryModuleList.First().Value.Guns.First().AmmoList.Last();
            // var shell = AppDataHelper.Instance.GetProjectile<ArtilleryShell>(shellName);
            // var dispersionData = ship.MainBatteryModuleList.First().Value.DispersionValues;
            // var sigma = ship.MainBatteryModuleList.First().Value.Sigma;
            // var maxRange = ship.MainBatteryModuleList.First().Value.MaxRange;
            // var aimingRange = 15000;
            // DispersionPlotParameters = DispersionPlotHelper.CalculateDispersionPlotParameters(dispersionData, shell, (double)maxRange, aimingRange, (double)sigma, Shots);
            // PlotScaling = 0.5;
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

        private double plotScaling;

        public double PlotScaling
        {
            get => plotScaling;
            set => this.RaiseAndSetIfChanged(ref plotScaling, value);
        }

        public void Apply()
        {
            // var ship = DataHelper.LoadPreviewShip(ShipClass.Battleship, 10, Nation.Japan).Ship;
            // var shellName = ship.MainBatteryModuleList.First().Value.Guns.First().AmmoList.Last();
            // var shell = AppDataHelper.Instance.GetProjectile<ArtilleryShell>(shellName);
            // var dispersionData = ship.MainBatteryModuleList.First().Value.DispersionValues;
            // var sigma = ship.MainBatteryModuleList.First().Value.Sigma;
            // var maxRange = ship.MainBatteryModuleList.First().Value.MaxRange;
            // var aimingRange = 15000;
            // DispersionPlotParameters = DispersionPlotHelper.CalculateDispersionPlotParameters(dispersionData, shell, (double)maxRange, aimingRange, (double)sigma, Shots);
            // PlotScaling = 0.5;
        }
    }
}
