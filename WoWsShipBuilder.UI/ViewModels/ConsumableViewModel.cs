using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class ConsumableViewModel : ViewModelBase
    {
        public ConsumableViewModel()
            : this(new Ship())
        {
        }

        public ConsumableViewModel(Ship ship)
        {
            ShipConsumables = ship.ShipConsumable.GroupBy(consumable => consumable.Slot)
                .OrderBy(group => group.Key)
                .Select(group => group.OrderBy(c => c.ConsumableName).Select(c =>
                        ConsumableUI.FromTypeAndVariant(c.ConsumableName, c.ConsumableVariantName, c.Slot, new List<(string name, float value)>(), false))
                    .ToList())
                .ToList();

            SelectedConsumables = new AvaloniaList<ConsumableUI>(ShipConsumables.Select(list => list.First()));
            OnConsumableSelected = newConsumable =>
            {
                var removeList = SelectedConsumables.Where(consumable => consumable.Slot == newConsumable.Slot);
                SelectedConsumables.RemoveAll(removeList);
                SelectedConsumables.Add(newConsumable);
            };
        }

        public List<List<ConsumableUI>> ShipConsumables { get; }

        public AvaloniaList<ConsumableUI> SelectedConsumables { get; }

        public Action<ConsumableUI> OnConsumableSelected { get; }
    }
}
