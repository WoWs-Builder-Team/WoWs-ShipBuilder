using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using ConsumableDataContainer = WoWsShipBuilder.DataContainers.ConsumableDataContainer;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public class ConsumableSlotViewModel : ReactiveObject
{
    private readonly Action<int, bool>? activationChangeHandler;
    private readonly List<ShipConsumable> shipConsumables;
    private readonly ILogger<ConsumableSlotViewModel> logger;

    private bool consumableActivated;

    private List<ConsumableDataContainer> consumableData = new();

    private int selectedIndex;

    public ConsumableSlotViewModel()
        : this(new List<ShipConsumable>(), null, NullLogger<ConsumableSlotViewModel>.Instance)
    {
    }

    private ConsumableSlotViewModel(IEnumerable<ShipConsumable> shipConsumables, Action<int, bool>? activationChangeHandler, ILogger<ConsumableSlotViewModel> logger)
    {
        this.activationChangeHandler = activationChangeHandler;
        this.logger = logger;
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
            logger.LogDebug("Selected consumable changed to index {Index}", value);
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

    public static ConsumableSlotViewModel Create(IEnumerable<ShipConsumable> shipConsumables, ILoggerFactory loggerFactory, ShipClass shipClass, Action<int, bool>? activationChangeHandler = null)
    {
        var vm = new ConsumableSlotViewModel(shipConsumables, activationChangeHandler, loggerFactory.CreateLogger<ConsumableSlotViewModel>());
        vm.UpdateDataContainers(new(), 0, shipClass);
        return vm;
    }

    public void UpdateDataContainers(List<(string, float)> modifiers, int shipHp, ShipClass shipClass)
    {
        var dataContainers = shipConsumables.Select(c => ConsumableDataContainer.FromTypeAndVariant(c, modifiers, false, shipHp, shipClass));
        ConsumableData = dataContainers.ToList();
    }
}
