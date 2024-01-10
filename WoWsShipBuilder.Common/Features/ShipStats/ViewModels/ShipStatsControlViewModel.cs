using System.Collections.Immutable;
using ReactiveUI;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public partial class ShipStatsControlViewModel : ReactiveObject
{
    private ShipDataContainer? currentShipStats;

    private bool isSpecialAbilityActive;

    public ShipStatsControlViewModel(Ship ship)
    {
        this.BaseShipStats = ship;
    }

    public ShipDataContainer? CurrentShipStats
    {
        get => this.currentShipStats;
        set => this.RaiseAndSetIfChanged(ref this.currentShipStats, value);
    }

    public bool IsSpecialAbilityActive
    {
        get => this.isSpecialAbilityActive;
        set => this.RaiseAndSetIfChanged(ref this.isSpecialAbilityActive, value);
    }

    // this is the ship base stats. do not modify after creation
    private Ship BaseShipStats { get; set; }

    public async Task UpdateShipStats(ImmutableList<ShipUpgrade> selectedConfiguration, ImmutableList<Modifier> modifiers)
    {
        var shipStats = await Task.Run(() => ShipDataContainer.CreateFromShip(this.BaseShipStats, selectedConfiguration, modifiers));
        this.CurrentShipStats = shipStats;
    }

    public List<Modifier> GetSpecialAbilityModifiers()
    {
        if (this.isSpecialAbilityActive )
        {
            if (this.BaseShipStats.SpecialAbility is not null)
            {
                return this.BaseShipStats.SpecialAbility.Modifiers.ToList();
            }
            else if (this.BaseShipStats.MainBatteryModuleList.FirstOrDefault().Value.BurstModeAbility is not null)
            {
                return this.BaseShipStats.MainBatteryModuleList.FirstOrDefault().Value.BurstModeAbility!.Modifiers.ToList();
            }
        }

        return new();
    }
}
