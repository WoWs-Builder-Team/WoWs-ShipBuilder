using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using WoWsShipBuilder.Common.Infrastructure;

namespace WoWsShipBuilder.ViewModels.Helper;

public partial class ValueSelectionViewModel : ViewModelBase
{
    public ValueSelectionViewModel()
        : this("Test", "Placeholder", new() { "item1", "item2" })
    {
    }

    public ValueSelectionViewModel(string text, string itemPlaceholderText, List<string> items)
    {
        Text = text;
        ItemPlaceholderText = itemPlaceholderText;
        Items = items;

        IObservable<bool> canOkExecute = this.WhenAnyValue(x => x.SelectedItem, selector: selected => selected != null);
        OkCommand = ReactiveCommand.CreateFromTask(Ok, canOkExecute);
    }

    [Observable]
    private string text = default!;

    [Observable]
    private string itemPlaceholderText = default!;

    [Observable]
    private List<string> items = new();

    [Observable]
    private string? selectedItem;

    public Interaction<string?, Unit> ConfirmationInteraction { get; } = new();

    public ICommand OkCommand { get; }

    private async Task Ok()
    {
        await ConfirmationInteraction.Handle(SelectedItem);
    }

    public async void Cancel()
    {
        await ConfirmationInteraction.Handle(null);
    }
}
