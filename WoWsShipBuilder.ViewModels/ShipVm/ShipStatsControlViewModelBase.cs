using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm
{
    public class ShipStatsControlViewModelBase : ViewModelBase
    {
        private readonly IAppDataService appDataService;

        private readonly ILocalizer localizer;

        public ShipStatsControlViewModelBase(Ship ship, List<ShipUpgrade> selectedConfiguration, List<(string, float)> modifiers, IAppDataService appDataService, ILocalizer localizer)
        {
            this.appDataService = appDataService;
            this.localizer = localizer;
            BaseShipStats = ship;
            Task.Run(async () => currentShipStats = await ShipDataContainer.FromShip(BaseShipStats, selectedConfiguration, modifiers, appDataService, localizer));
        }

        private ShipDataContainer? currentShipStats;

        public ShipDataContainer? CurrentShipStats
        {
            get => currentShipStats;
            set => this.RaiseAndSetIfChanged(ref currentShipStats, value);
        }

        // this is the ship base stats. do not modify after creation
        private Ship BaseShipStats { get; set; }

        public async Task UpdateShipStats(List<ShipUpgrade> selectedConfiguration, List<(string, float)> modifiers)
        {
            ShipDataContainer shipStats = await Task.Run(() => ShipDataContainer.FromShip(BaseShipStats, selectedConfiguration, modifiers, appDataService, localizer));
            CurrentShipStats = shipStats;
        }
    }
}
