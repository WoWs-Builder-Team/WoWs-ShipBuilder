using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm
{
    internal class DpmAngleViewModel : ViewModelBase
    {
        public DpmAngleViewModel(TurretModule turrets, TorpedoModule torpModule)
        {
            Turrets = turrets;
            TorpModule = torpModule;
        }

        private TurretModule turret = null!;

        public TurretModule Turrets
        {
            get => turret;
            set => this.RaiseAndSetIfChanged(ref turret, value);
        }

        private TorpedoModule torpModule = null!;

        public TorpedoModule TorpModule
        {
            get => torpModule;
            set => this.RaiseAndSetIfChanged(ref torpModule, value);
        }
    }
}
