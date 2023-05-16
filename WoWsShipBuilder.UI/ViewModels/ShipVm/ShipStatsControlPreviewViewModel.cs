using System;
using Avalonia.Controls;
using WoWsShipBuilder.Common.Features.ShipStats.ViewModels;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.UI.ViewModels.ShipVm;

public class ShipStatsControlPreviewViewModel : ShipStatsControlViewModel
{
    public ShipStatsControlPreviewViewModel()
        : base(DesignDataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Germany).Ship)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor must not be used outside of design mode.");
        }
    }
}
