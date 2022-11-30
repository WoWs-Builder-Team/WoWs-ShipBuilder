using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Settings;

namespace WoWsShipBuilder.Web.ViewModels;

using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.ViewModels.ShipVm;

public class ShipViewModel : MainWindowViewModelBase
{
    public ShipViewModel(INavigationService navigationService, ILocalizer localizer, AppSettings appSettings, MainViewModelParams viewModelParams)
        : base(navigationService, localizer, appSettings, viewModelParams)
    {
    }
}
