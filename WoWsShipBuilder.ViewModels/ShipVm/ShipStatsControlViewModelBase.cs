using ReactiveUI;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm;

public class ShipStatsControlViewModel : ViewModelBase
{
    private readonly IAppDataService appDataService;

    public ShipStatsControlViewModel(Ship ship, IAppDataService appDataService)
    {
        this.appDataService = appDataService;
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
        ShipDataContainer shipStats = await Task.Run(() => ShipDataContainer.FromShipAsync(BaseShipStats, selectedConfiguration, modifiers, appDataService));
        CurrentShipStats = shipStats;
    }
}
