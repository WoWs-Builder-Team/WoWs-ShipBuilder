using System.Collections.Generic;
using WoWsShipBuilder.Common.Features.ShipStats.ViewModels;
using WoWsShipBuilder.Common.Infrastructure.Data;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class UpgradePanelViewModel : UpgradePanelViewModelBase
    {
        public UpgradePanelViewModel()
            : this(DesignDataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Germany).Ship, AppData.ModernizationCache ?? new Dictionary<string, Modernization>())
        {
        }

        public UpgradePanelViewModel(Ship ship, Dictionary<string, Modernization> upgradeData)
            : base(ship, upgradeData)
        {
        }
    }
}
