using System.Collections.Generic;
using ReactiveUI;
using WoWsShipBuilder.Infrastructure;
using DepthChargeDataContainer = WoWsShipBuilder.DataContainers.DepthChargeDataContainer;

namespace WoWsShipBuilder.Desktop.ViewModels
{
    public class TestWindowViewModel : ViewModelBase
    {
        public TestWindowViewModel(DepthChargeDataContainer depthCharge)
        {
            DcDmg = depthCharge.Damage;
            SplashRadius = depthCharge.DcSplashRadius;
            PointsOfDmg = depthCharge.PointsOfDmg;
        }

        private int dcDmg;

        public int DcDmg
        {
            get => dcDmg;
            set => this.RaiseAndSetIfChanged(ref dcDmg, value);
        }

        private decimal splashRadius;

        public decimal SplashRadius
        {
            get => splashRadius;
            set => this.RaiseAndSetIfChanged(ref splashRadius, value);
        }

        private Dictionary<float, List<float>> pointsOfDmg = default!;

        public Dictionary<float, List<float>> PointsOfDmg
        {
            get => pointsOfDmg;
            set => this.RaiseAndSetIfChanged(ref pointsOfDmg, value);
        }
    }
}
