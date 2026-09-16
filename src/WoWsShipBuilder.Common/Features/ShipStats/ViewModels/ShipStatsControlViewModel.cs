using System.Collections.Immutable;
using ReactiveUI;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.DataContainers;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public partial class ShipStatsControlViewModel(Ship ship) : ReactiveObject
{
    [Observable]
    private ShipDataContainer? currentShipStats;

    [Observable]
    private bool isSpecialAbilityActive;

    private Ship BaseShipStats { get; } = ship;

    public async Task UpdateShipStats(ImmutableList<ShipUpgrade> selectedConfiguration, ImmutableList<Modifier> modifiers)
    {
        var shipStats = await Task.Run(() => ShipDataContainer.CreateFromShip(this.BaseShipStats, selectedConfiguration, modifiers));
        this.CurrentShipStats = shipStats;
    }

    public List<Modifier> GetSpecialAbilityModifiers()
    {
        if (this.IsSpecialAbilityActive)
        {
            if (this.BaseShipStats.SpecialAbility is not null)
            {
                return [.. this.BaseShipStats.SpecialAbility.Modifiers];
            }

            if (this.BaseShipStats.MainBatteryModuleList.FirstOrDefault().Value.BurstModeAbility is { } burstModeAbility)
            {
                return [.. burstModeAbility.Modifiers];
            }
        }

        return [];
    }
}
