using WoWsShipBuilder.Core.Localization;

namespace WoWsShipBuilder.Web.ViewModels;

using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.ViewModels.ShipVm;

public class ShipViewModel : ShipViewModelBase
{
    public ShipViewModel(INavigationService navigationService, ILocalizer localizer, ILogger<ShipViewModel> logger, ShipViewModelParams viewModelParams)
        : base(navigationService, localizer, logger, viewModelParams)
    {
    }
}
