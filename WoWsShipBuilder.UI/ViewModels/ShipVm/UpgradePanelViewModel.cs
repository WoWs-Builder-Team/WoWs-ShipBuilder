using System.Collections.Generic;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class UpgradePanelViewModel : UpgradePanelViewModelBase
    {
        public UpgradePanelViewModel()
            : this(DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Germany).Ship, LoadParamsAsync(DesktopAppDataService.PreviewInstance, AppSettingsHelper.Settings).Result)
        {
        }

        public UpgradePanelViewModel(Ship ship, Dictionary<string, Modernization> upgradeData)
            : base(ship, upgradeData)
        {
        }
    }
}
