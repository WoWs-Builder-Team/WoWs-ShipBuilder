using System.Collections.Generic;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class UpgradePanelViewModel : UpgradePanelViewModelBase
    {
        public UpgradePanelViewModel()
            : this(DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Germany).Ship, AppData.ModernizationCache ?? new Dictionary<string, Modernization>())
        {
        }

        public UpgradePanelViewModel(Ship ship, Dictionary<string, Modernization> upgradeData)
            : base(ship, upgradeData)
        {
        }
    }
}
