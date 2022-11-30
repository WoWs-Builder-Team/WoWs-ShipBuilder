using System;
using ReactiveUI;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm;

public class ConsumableSlotViewModel : ViewModelBase
{
    private readonly Action<int, bool>? activationChangeHandler;
    private readonly List<ShipConsumable> shipConsumables;

    private bool consumableActivated;

    private List<ConsumableDataContainer> consumableData = new();

    private int selectedIndex;

    public ConsumableSlotViewModel()
        : this(new List<ShipConsumable>(), null)
    {
    }

    private ConsumableSlotViewModel(IEnumerable<ShipConsumable> shipConsumables, Action<int, bool>? activationChangeHandler)
    {
        this.activationChangeHandler = activationChangeHandler;
        this.shipConsumables = new(shipConsumables);
        Slot = this.shipConsumables.First().Slot;
    }

    public int Slot { get; }

    public List<ConsumableDataContainer> ConsumableData
    {
        get => consumableData;
        set
        {
            this.RaiseAndSetIfChanged(ref consumableData, value);
            this.RaisePropertyChanged(nameof(SelectedIndex)); // Necessary to update index for selection popup
        }
    }

    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedIndex, value);
            this.RaisePropertyChanged(nameof(SelectedConsumable));
            ConsumableActivated = false;
        }
    }

    public bool ConsumableActivated
    {
        get => consumableActivated;
        set
        {
            this.RaiseAndSetIfChanged(ref consumableActivated, value);
            activationChangeHandler?.Invoke(Slot, value);
        }
    }

    public ConsumableDataContainer SelectedConsumable => ConsumableData[SelectedIndex];

    public static ConsumableSlotViewModel Create(IEnumerable<ShipConsumable> shipConsumables, Action<int, bool>? activationChangeHandler = null)
    {
        var vm = new ConsumableSlotViewModel(shipConsumables, activationChangeHandler);
        vm.UpdateDataContainers(new(), 0);
        return vm;
    }

    public void UpdateDataContainers(List<(string, float)> modifiers, int shipHp)
    {
        var dataContainers = shipConsumables.Select(c => ConsumableDataContainer.FromTypeAndVariant(c, modifiers, false, 0, shipHp));
        ConsumableData = dataContainers.ToList();
    }
}
