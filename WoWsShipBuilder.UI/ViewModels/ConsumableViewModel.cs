using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using ReactiveUI;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class ConsumableViewModel : ViewModelBase, IBuildStorable
    {
        private List<List<ConsumableUI>> shipConsumables = null!;

        private Ship ship;

        public ConsumableViewModel()
            : this(new Ship())
        {
        }

        public ConsumableViewModel(Ship ship)
        {
            this.ship = ship;
            SelectedConsumables = new AvaloniaList<ConsumableUI>();
            UpdateShipConsumables(new List<(string, float)>(), true);
            OnConsumableSelected = newConsumable =>
            {
                var removeList = SelectedConsumables.Where(consumable => consumable.Slot == newConsumable.Slot).ToList();
                SelectedConsumables.RemoveAll(removeList);
                SelectedConsumables.Add(newConsumable);
                this.RaisePropertyChanged(nameof(SelectedConsumables));
            };
        }

        public void UpdateShipConsumables(List<(string, float)> modifiers, bool isFirstTry = false)
        {
            ShipConsumables = ship.ShipConsumable.GroupBy(consumable => consumable.Slot)
                .OrderBy(group => group.Key)
                .Select(group => group.OrderBy(c => c.ConsumableName).Select(c =>
                        ConsumableUI.FromTypeAndVariant(c.ConsumableName, c.ConsumableVariantName, c.Slot, modifiers, false))
                    .ToList())
                .ToList();

            List<ConsumableUI> newSelection = new();
            if (isFirstTry)
            {
                newSelection = ShipConsumables.Select(list => list.First()).ToList();
            }
            else
            {
                foreach (ConsumableUI selectedConsumable in SelectedConsumables)
                {
                    newSelection.Add(ShipConsumables.SelectMany(list => list).First(consumable => consumable.IconName.Equals(selectedConsumable.IconName)));
                }

                SelectedConsumables.Clear();
            }

            SelectedConsumables.AddRange(newSelection);
        }

        public List<List<ConsumableUI>> ShipConsumables
        {
            get => shipConsumables;
            set => this.RaiseAndSetIfChanged(ref shipConsumables, value);
        }

        public AvaloniaList<ConsumableUI> SelectedConsumables { get; private set; }

        public Action<ConsumableUI> OnConsumableSelected { get; }

        public void LoadBuild(IEnumerable<string> storedData)
        {
            var selection = new List<ConsumableUI>();
            foreach (List<ConsumableUI> consumableList in ShipConsumables)
            {
                selection.AddRange(consumableList.Where(consumable => storedData.Contains(consumable.IconName)));
            }

            var removeList = SelectedConsumables.Where(selected => selection.Any(newSelected => newSelected.Slot == selected.Slot)).ToList();
            SelectedConsumables.RemoveAll(removeList);
            SelectedConsumables.AddRange(selection);
            this.RaisePropertyChanged(nameof(SelectedConsumables));
        }

        public List<string> SaveBuild()
        {
            return SelectedConsumables.Select(consumable => consumable.IconName).ToList();
        }
    }
}
