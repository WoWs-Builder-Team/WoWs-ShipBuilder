using System.Collections.Generic;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class ShipStatsControlViewModel : ShipStatsControlViewModelBase
    {
        public ShipStatsControlViewModel()
        : this(DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Germany))
        {
        }

        public ShipStatsControlViewModel(Ship ship, List<ShipUpgrade> selectedConfiguration, List<(string, float)> modifiers, IAppDataService appDataService)
            : base(ship, selectedConfiguration, modifiers, appDataService)
        {
        }

        private ShipStatsControlViewModel((Ship Ship, List<ShipUpgrade> Configuration) shipData)
            : this(shipData.Ship, shipData.Configuration, new(), DesktopAppDataService.PreviewInstance)
        {
        }
    }
}
