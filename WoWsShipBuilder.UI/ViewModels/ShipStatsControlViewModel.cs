using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class ShipStatsControlViewModel : ViewModelBase
    {
        public ShipStatsControlViewModel(Ship ship, List<ShipUpgrade> selectedConfiguration, List<(string, float)> modifiers)
        {
            BaseShipStats = ship;
            currentShipStats = ShipUI.FromShip(BaseShipStats, selectedConfiguration, modifiers);
        }

        public ShipStatsControlViewModel()
        {
            var testData = DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10);
            BaseShipStats = testData.Ship;
            currentShipStats = ShipUI.FromShip(BaseShipStats, testData.Configuration, new List<(string, float)>());
        }

        private ShipUI? currentShipStats;

        public ShipUI? CurrentShipStats
        {
            get => currentShipStats;
            set => this.RaiseAndSetIfChanged(ref currentShipStats, value);
        }

        // this is the ship base stats. do not modify after creation
        private Ship BaseShipStats { get; set; }

        public async Task UpdateShipStats(List<ShipUpgrade> selectedConfiguration, List<(string, float)> modifiers)
        {
            ShipUI shipStats = await Task.Run(() => ShipUI.FromShip(BaseShipStats, selectedConfiguration, modifiers));
            CurrentShipStats = shipStats;
        }
    }
}
