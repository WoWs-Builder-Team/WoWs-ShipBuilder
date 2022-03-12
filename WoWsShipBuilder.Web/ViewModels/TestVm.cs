using ReactiveUI;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.Web.ViewModels;

public class TestVm : ViewModelBase
{
    public TestVm(string initialSelect)
    {
        selectedItem = initialSelect;
    }

    private string selectedItem;

    public string SelectedItem
    {
        get => selectedItem;
        set => this.RaiseAndSetIfChanged(ref selectedItem, value);
    }
}
