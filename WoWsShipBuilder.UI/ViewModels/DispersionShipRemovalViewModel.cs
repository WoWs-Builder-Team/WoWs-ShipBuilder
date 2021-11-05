using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Metadata;
using ReactiveUI;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class DispersionShipRemovalViewModel : ViewModelBase
    {
        public DispersionShipRemovalViewModel(List<string> currentShipList)
        {
            CurrentShipList = new AvaloniaList<string>(currentShipList);
            CurrentSelection.CollectionChanged += CurrentSelection_CollectionChanged;
            RemoveSelection.CollectionChanged += RemoveSelection_CollectionChanged;
        }

        private void RemoveSelection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(RemoveSelection));
        }

        private void CurrentSelection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(CurrentSelection));
        }

        private AvaloniaList<string> currentShipList = new();

        public AvaloniaList<string> CurrentShipList
        {
            get => currentShipList;
            set => this.RaiseAndSetIfChanged(ref currentShipList, value);
        }

        private AvaloniaList<string> shipsToDeleteList = new();

        public AvaloniaList<string> ShipsToDeleteList
        {
            get => shipsToDeleteList;
            set => this.RaiseAndSetIfChanged(ref shipsToDeleteList, value);
        }

        private AvaloniaList<string> currentSelection = new();

        public AvaloniaList<string> CurrentSelection
        {
            get => currentSelection;
            set => this.RaiseAndSetIfChanged(ref currentSelection, value);
        }

        private AvaloniaList<string> removeSelection = new();

        public AvaloniaList<string> RemoveSelection
        {
            get => removeSelection;
            set => this.RaiseAndSetIfChanged(ref removeSelection, value);
        }

        public void RemoveShips()
        {
            var curr = CurrentSelection.ToList();
            ShipsToDeleteList.AddRange(curr);
            CurrentShipList.RemoveAll(curr);
        }

        // TODO: Change into something that does listen to CollectionChanged events.
        [DependsOn(nameof(CurrentSelection))]
        public bool CanRemoveShips(object parameter) => CurrentSelection.Count > 0;

        public void RestoreShips()
        {
            var rem = RemoveSelection.ToList();
            CurrentShipList.AddRange(rem);
            ShipsToDeleteList.RemoveAll(rem);
        }

        [DependsOn(nameof(RemoveSelection))]
        public bool CanRestoreShips(object parameter) => RemoveSelection.Count > 0;
    }
}
