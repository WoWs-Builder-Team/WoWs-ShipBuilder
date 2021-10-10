using ReactiveUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class ShipStatsControlViewModel : ViewModelBase
    {
        public ShipStatsControlViewModel(Ship ship)
        {
            CurrentShipStats = ship;
            BaseShipStats = ship;
        }

        public ShipStatsControlViewModel()
            : this(new Ship())
        {
        }

        private Ship? currentShipStats;

        public Ship? CurrentShipStats
        {
            get => currentShipStats;
            set => this.RaiseAndSetIfChanged(ref currentShipStats, value);
        }

        // this is the ship base stats. do not modify after creation
        private Ship? BaseShipStats { get; set; }
    }
}
