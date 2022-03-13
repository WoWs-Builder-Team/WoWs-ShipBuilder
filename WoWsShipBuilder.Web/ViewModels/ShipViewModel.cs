namespace WoWsShipBuilder.Web.ViewModels;

using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.ViewModels.ShipVm;

public class ShipViewModel : MainWindowViewModelBase
{
    public ShipViewModel(INavigationService navigationService, IAppDataService appDataService, MainViewModelParams viewModelParams)
        : base(navigationService, appDataService, viewModelParams)
    {
    }

    public override void OpenSaveBuild()
    {
        throw new NotImplementedException();
    }
}
