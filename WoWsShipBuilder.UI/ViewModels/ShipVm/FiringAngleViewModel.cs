using System.Collections.Generic;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Components;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class FiringAngleViewModel : FiringAngleViewModelBase
    {
        public FiringAngleViewModel(IEnumerable<IGun> guns)
            : base(guns)
        {
        }

        public FiringAngleViewModel()
            : this(DataHelper.GetPreviewTurretModule(ShipClass.Battleship, 10, Nation.Germany).Guns)
        {
        }
    }
}
