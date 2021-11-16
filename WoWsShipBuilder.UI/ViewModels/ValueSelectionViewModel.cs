using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Metadata;
using ReactiveUI;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class ValueSelectionViewModel : ViewModelBase
    {
        private Window self;

        public ValueSelectionViewModel(Window win, string text, string itemPlaceholderText, List<string> items)
        {
            Text = text;
            ItemPlaceholderText = itemPlaceholderText;
            Items = items;
            self = win;
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
       
        public void Ok(object parameter)
        {
            self.Close(SelectedItem);
        }

        [DependsOn(nameof(SelectedItem))]
        public bool CanOk(object parameter)
        {
            return SelectedItem != null;
        }

        public void Cancel()
        {
            self.Close();
        }
    }
}
