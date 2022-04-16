using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm
{
    public class ConsumableViewModel : ViewModelBase, IBuildStorable
    {
        private readonly Ship ship;

        private List<List<ConsumableUI>> shipConsumables;

        public ConsumableViewModel()
            : this(new())
        {
        }

        private ConsumableViewModel(Ship ship)
        {
            this.ship = ship;
            shipConsumables = new();
            SelectedConsumables = new();

            // TODO: get rid of callback property
            OnConsumableSelected = newConsumable =>
            {
                var removeList = SelectedConsumables.Where(consumable => consumable.Slot == newConsumable.Slot).ToList();
                SelectedConsumables.RemoveMany(removeList);
                SelectedConsumables.Add(newConsumable);
                this.RaisePropertyChanged(nameof(SelectedConsumables));
            };
        }

        public static async Task<ConsumableViewModel> CreateAsync(Ship ship, int shipHp)
        {
            var vm = new ConsumableViewModel(ship);
            await vm.UpdateShipConsumables(new(), shipHp, true);
            return vm;
        }

        public async Task UpdateShipConsumables(List<(string, float)> modifiers, int shipHp, bool isFirstTry = false)
        {
            ShipConsumables = (await Task.WhenAll(ship.ShipConsumable.GroupBy(consumable => consumable.Slot)
                    .OrderBy(group => group.Key)
                    .Select(async group => (await Task.WhenAll(group.OrderBy(c => c.ConsumableName).Select(async c =>
                            await ConsumableUI.FromTypeAndVariant(c.ConsumableName, c.ConsumableVariantName, c.Slot, modifiers, false, 0, shipHp))))
                        .ToList())))
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

        public CustomObservableCollection<ConsumableUI> SelectedConsumables { get; private set; }

        public Action<ConsumableUI> OnConsumableSelected { get; }

        public void LoadBuild(IEnumerable<string> storedData)
        {
            var selection = new List<ConsumableUI>();
            foreach (List<ConsumableUI> consumableList in ShipConsumables)
            {
                selection.AddRange(consumableList.Where(consumable => storedData.Contains(consumable.IconName)));
            }

            var removeList = SelectedConsumables.Where(selected => selection.Any(newSelected => newSelected.Slot == selected.Slot)).ToList();
            SelectedConsumables.RemoveMany(removeList);
            SelectedConsumables.AddRange(selection);
            this.RaisePropertyChanged(nameof(SelectedConsumables));
        }

        public List<string> SaveBuild()
        {
            return SelectedConsumables.Select(consumable => consumable.IconName).ToList();
        }
    }
}
