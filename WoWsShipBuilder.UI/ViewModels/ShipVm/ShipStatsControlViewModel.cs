using System.Collections.Generic;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class ShipStatsControlViewModel : ShipStatsControlViewModelBase
    {
        public ShipStatsControlViewModel()
        : this(DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Germany), DataHelper.DemoLocalizer)
        {
        }

        public ShipStatsControlViewModel(Ship ship, List<ShipUpgrade> selectedConfiguration, List<(string, float)> modifiers, IAppDataService appDataService, ILocalizer localizer)
            : base(ship, selectedConfiguration, modifiers, appDataService, localizer)
        {
        }

        private ShipStatsControlViewModel((Ship Ship, List<ShipUpgrade> Configuration) shipData, ILocalizer localizer)
            : this(shipData.Ship, shipData.Configuration, new(), DesktopAppDataService.PreviewInstance, localizer)
        {
        }
    }
}
