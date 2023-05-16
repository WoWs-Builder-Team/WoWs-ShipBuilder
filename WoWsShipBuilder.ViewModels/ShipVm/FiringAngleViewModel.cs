using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.DataStructures.Ship.Components;

namespace WoWsShipBuilder.ViewModels.ShipVm;

public partial class FiringAngleViewModel : ViewModelBase
{
    [Observable]
    private IEnumerable<IGun> guns;

    [Observable]
    private bool permaText = true;

    // TODO: get rid of static translations
    [Observable]
    private string permaTextButton = Translation.FiringAngleWindow_PermaTextOff;

    [Observable]
    private bool showAllText;

    [Observable]
    private string showAllTextButton = Translation.FiringAngleWindow_ShowAll;

    public FiringAngleViewModel(IEnumerable<IGun> guns)
    {
        this.guns = guns;
    }

    public void SetShowAll()
    {
        if (ShowAllText)
        {
            ShowAllText = false;
            ShowAllTextButton = Translation.FiringAngleWindow_ShowAll;
        }
        else
        {
            ShowAllText = true;
            ShowAllTextButton = Translation.FiringAngleWindow_HideAll;
        }
    }

    public void SetPermaText()
    {
        if (PermaText)
        {
            PermaText = false;
            PermaTextButton = Translation.FiringAngleWindow_PermaTextOn;
        }
        else
        {
            PermaText = true;
            PermaTextButton = Translation.FiringAngleWindow_PermaTextOff;
        }
    }
}
