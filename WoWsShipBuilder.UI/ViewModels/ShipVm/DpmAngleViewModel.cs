using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm
{
    public class DpmAngleViewModel : DpmAngleViewModelBase
    {
        public DpmAngleViewModel(TurretModule turretModule, TorpedoModule torpModule)
            : base(turretModule, torpModule)
        {
        }
    }
}
