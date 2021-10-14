using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using WoWsShipBuilder.Core.DataProvider;
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
            var consumableData = AppDataHelper.Instance.ReadLocalJsonData<Consumable>(Nation.Common, ServerType.Live)!;
            ShipConsumables = ship.ShipConsumable.GroupBy(consumable => consumable.Slot)
                .OrderBy(group => group.Key)
                .Select(group => group.OrderBy(c => c.ConsumableName).Select(c => (c,
                    consumableData.Values.Single(rawConsumable =>
                        rawConsumable.Name.Equals(c.ConsumableName) && rawConsumable.ConsumableVariantName.Equals(c.ConsumableVariantName))))
                    .ToList())
                .ToList();

            SelectedConsumables = new AvaloniaList<(ShipConsumable, Consumable)>(ShipConsumables.Select(list => list.First()));
            OnConsumableSelected = _ => { }; // TODO: add logic
        }

        public List<List<(ShipConsumable, Consumable)>> ShipConsumables { get; }

        public AvaloniaList<(ShipConsumable, Consumable)> SelectedConsumables { get; }

        public Action<ShipConsumable> OnConsumableSelected { get; }
    }
}
