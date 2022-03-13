namespace WoWsShipBuilder.Web.ViewModels;

using ReactiveUI;
using WoWsShipBuilder.ViewModels.Base;

public class TestVm : ViewModelBase
{
    private string selectedItem;

    public TestVm(string initialSelect)
    {
        selectedItem = initialSelect;
    }

    public string SelectedItem
    {
        get => selectedItem;
        set => this.RaiseAndSetIfChanged(ref selectedItem, value);
    }
}
