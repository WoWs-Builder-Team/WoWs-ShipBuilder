using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class UpgradePanelViewModel : UpgradePanelViewModelBase
    {
        public UpgradePanelViewModel()
            : this(DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Germany).Ship)
        {
        }

        public UpgradePanelViewModel(Ship ship)
            : base(ship)
        {
        }
    }
}
