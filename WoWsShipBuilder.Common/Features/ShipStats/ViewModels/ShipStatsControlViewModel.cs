using ReactiveUI;
using WoWsShipBuilder.Common.DataContainers;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Common.Features.ShipStats.ViewModels;

public class ShipStatsControlViewModel : ViewModelBase
{
    public ShipStatsControlViewModel(Ship ship)
    {
        BaseShipStats = ship;
    }

    private ShipDataContainer? currentShipStats;

    public ShipDataContainer? CurrentShipStats
    {
        get => currentShipStats;
        set => this.RaiseAndSetIfChanged(ref currentShipStats, value);
    }

    // this is the ship base stats. do not modify after creation
    private Ship BaseShipStats { get; set; }

    public async Task UpdateShipStats(List<ShipUpgrade> selectedConfiguration, List<(string, float)> modifiers)
    {
        ShipDataContainer shipStats = await Task.Run(() => ShipDataContainer.CreateFromShip(BaseShipStats, selectedConfiguration, modifiers));
        CurrentShipStats = shipStats;
    }
}
