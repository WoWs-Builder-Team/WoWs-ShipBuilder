using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class ValueSelectionViewModel : ViewModelBase
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

            IObservable<bool>? canOkExecute = this.WhenAnyValue(x => x.SelectedItem, selector: selected => selected != null);
            OkCommand = ReactiveCommand.CreateFromTask(Ok, canOkExecute);
        }

        private string text = default!;

        public string Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }

        private string itemPlaceholderText = default!;

        public string ItemPlaceholderText
        {
            get => itemPlaceholderText;
            set => this.RaiseAndSetIfChanged(ref itemPlaceholderText, value);
        }

        private List<string> items = new();

        public List<string> Items
        {
            get => items;
            set => this.RaiseAndSetIfChanged(ref items, value);
        }

        private string? selectedItem;

        public string? SelectedItem
        {
            get => selectedItem;
            set => this.RaiseAndSetIfChanged(ref selectedItem, value);
        }

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
}
