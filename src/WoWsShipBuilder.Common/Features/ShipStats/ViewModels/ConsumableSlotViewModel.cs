using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public class ConsumableSlotViewModel : ReactiveObject
{
    private readonly Action<int, bool>? activationChangeHandler;
    private readonly List<ShipConsumable> shipConsumables;
    private readonly ILogger<ConsumableSlotViewModel> logger;

    private bool consumableActivated;

    private List<ConsumableDataContainer> consumableData = new();

    private int selectedIndex;

    private ConsumableSlotViewModel(IEnumerable<ShipConsumable> shipConsumables, Action<int, bool>? activationChangeHandler, ILogger<ConsumableSlotViewModel> logger)
    {
        this.activationChangeHandler = activationChangeHandler;
        this.logger = logger;
        this.shipConsumables = new(shipConsumables);
        this.Slot = this.shipConsumables[0].Slot;
    }

    public int Slot { get; }

    public List<ConsumableDataContainer> ConsumableData
    {
        get => this.consumableData;
        set
        {
            this.RaiseAndSetIfChanged(ref this.consumableData, value);
            this.RaisePropertyChanged(nameof(this.SelectedIndex)); // Necessary to update index for selection popup
        }
    }

    public int SelectedIndex
    {
        get => this.selectedIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref this.selectedIndex, value);
            this.RaisePropertyChanged(nameof(this.SelectedConsumable));
            this.logger.LogDebug("Selected consumable changed to index {Index}", value);
            this.ConsumableActivated = false;
        }
    }

    public bool ConsumableActivated
    {
        get => this.consumableActivated;
        set
        {
            this.RaiseAndSetIfChanged(ref this.consumableActivated, value);
            this.activationChangeHandler?.Invoke(this.Slot, value);
        }
    }

    public ConsumableDataContainer SelectedConsumable => this.ConsumableData[this.SelectedIndex];

    public static ConsumableSlotViewModel Create(IEnumerable<ShipConsumable> shipConsumables, ILoggerFactory loggerFactory, ShipClass shipClass, Action<int, bool>? activationChangeHandler = null)
    {
        var vm = new ConsumableSlotViewModel(shipConsumables, activationChangeHandler, loggerFactory.CreateLogger<ConsumableSlotViewModel>());
        vm.UpdateDataContainers(ImmutableList<Modifier>.Empty, 0, shipClass);
        return vm;
    }

    public void UpdateDataContainers(ImmutableList<Modifier> modifiers, int shipHp, ShipClass shipClass)
    {
        var dataContainers = this.shipConsumables.Select(c => ConsumableDataContainer.FromTypeAndVariant(c, modifiers, false, shipHp, shipClass));
        this.ConsumableData = dataContainers.ToList();
    }
}
