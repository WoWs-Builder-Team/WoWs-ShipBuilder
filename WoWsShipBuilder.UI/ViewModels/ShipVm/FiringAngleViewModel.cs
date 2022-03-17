using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class FiringAngleViewModel : FiringAngleViewModelBase
    {
        public FiringAngleViewModel(TurretModule turretModule)
            : base(turretModule)
        {
        }

        public FiringAngleViewModel()
            : this(DataHelper.GetPreviewTurretModule(ShipClass.Battleship, 10, Nation.Germany))
        {
        }
    }
}
