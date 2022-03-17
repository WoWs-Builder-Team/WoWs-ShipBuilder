using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.UI.ViewModels.DispersionPlot
{
    public class DispersionShipRemovalViewModel : ViewModelBase
    {
        public DispersionShipRemovalViewModel()
            : this(new())
        {
        }

        public DispersionShipRemovalViewModel(List<string> currentShipList)
        {
            CurrentShipList = new(currentShipList);
            CurrentSelection.CollectionChanged += CurrentSelection_CollectionChanged;
            RemoveSelection.CollectionChanged += RemoveSelection_CollectionChanged;

            IObservable<bool> canRemoveExecute = CurrentSelection.ToObservableChangeSet().Select(_ => CurrentSelection.Any());
            IObservable<bool> canRestoreExecute = RemoveSelection.ToObservableChangeSet().Select(_ => RemoveSelection.Any());
            RemoveShipsCommand = ReactiveCommand.Create(RemoveShips, canRemoveExecute);
            RestoreShipsCommand = ReactiveCommand.Create(RestoreShips, canRestoreExecute);
        }

        public ReactiveCommand<Unit, Unit> RemoveShipsCommand { get; }

        public ReactiveCommand<Unit, Unit> RestoreShipsCommand { get; }

        private void RemoveSelection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(RemoveSelection));
        }

        private void CurrentSelection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(CurrentSelection));
        }

        private CustomObservableCollection<string> currentShipList = new();

        public CustomObservableCollection<string> CurrentShipList { get; }

        public CustomObservableCollection<string> ShipsToDeleteList { get; } = new();

        public CustomObservableCollection<string> CurrentSelection { get; } = new();

        public CustomObservableCollection<string> RemoveSelection { get; } = new();

        public void RemoveShips()
        {
            var curr = CurrentSelection.ToList();
            ShipsToDeleteList.AddRange(curr);
            CurrentShipList.RemoveMany(curr);
        }

        public void RestoreShips()
        {
            var rem = RemoveSelection.ToList();
            CurrentShipList.AddRange(rem);
            ShipsToDeleteList.RemoveMany(rem);
        }
    }
}
