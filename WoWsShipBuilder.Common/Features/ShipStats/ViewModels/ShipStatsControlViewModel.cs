using ReactiveUI;
using WoWsShipBuilder.DataContainers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public class ShipStatsControlViewModel : ViewModelBase
{
    private ShipDataContainer? currentShipStats;

    public ShipStatsControlViewModel(Ship ship)
    {
        BaseShipStats = ship;
    }

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
