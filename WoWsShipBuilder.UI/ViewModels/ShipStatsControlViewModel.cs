using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class ShipStatsControlViewModel : ViewModelBase
    {
        private readonly IAppDataService appDataService;

        public ShipStatsControlViewModel(Ship ship, List<ShipUpgrade> selectedConfiguration, List<(string, float)> modifiers, IAppDataService appDataService)
        {
            this.appDataService = appDataService;
            BaseShipStats = ship;
            currentShipStats = ShipUI.FromShip(BaseShipStats, selectedConfiguration, modifiers, appDataService);
        }

        public ShipStatsControlViewModel()
        {
            appDataService = DesktopAppDataService.PreviewInstance;
            var testData = DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Germany);
            BaseShipStats = testData.Ship;
            currentShipStats = ShipUI.FromShip(BaseShipStats, testData.Configuration, new(), appDataService);
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
            ShipUI shipStats = await Task.Run(() => ShipUI.FromShip(BaseShipStats, selectedConfiguration, modifiers, appDataService));
            CurrentShipStats = shipStats;
        }
    }
}
