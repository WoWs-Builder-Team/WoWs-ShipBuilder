using ReactiveUI;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class ShipStatsControlViewModel : ViewModelBase
    {
        public ShipStatsControlViewModel(Ship ship)
        {
            // CurrentShipStats = ship;
            BaseShipStats = ship;
            SecondaryBatteryUI secondaryBatteryUI = new SecondaryBatteryUI();
            secondaryBatteryUI.SecondaryName = "test";
            secondaryBatteryUI.SecondaryRange = 10;
            secondaryBatteryUI.SecondaryDamage = 1000;
            secondaryBatteryUI.SecondaryPenetration = 100;
            secondaryBatteryUI.SecondaryFireChance = 10;
            secondaryBatteryUI.SecondaryReload = 5;
            secondaryBatteryUI.SecondaryRoF = 12;
            secondaryBatteryUI.SecondarySigma = 1.0m;
            secondaryBatteryUI.SecondaryHorizontalDisp = 100;
            secondaryBatteryUI.SecondaryVerticalDisp = 100;
            secondaryBatteryUI.SecondaryShellVelocity = 1000;

            // CurrentShipStats.SecondaryBatteryUI.Add(secondaryBatteryUI);
        }

        public ShipStatsControlViewModel()
            : this(new Ship())
        {
        }

        private ShipUI? currentShipStats;

        public ShipUI? CurrentShipStats
        {
            get => currentShipStats;
            set => this.RaiseAndSetIfChanged(ref currentShipStats, value);
        }

        // this is the ship base stats. do not modify after creation
        private Ship? BaseShipStats { get; set; }
    }
}
