using System.Collections.ObjectModel;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveUI;
using WoWsShipBuilder.DataContainers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

/// <summary>
/// Represents the consumable slots of a ship.
/// Data for each slot is stored in an individual <see cref="ConsumableSlotViewModel"/>
/// while this viewmodel is responsible to store these individual instances and provide methods to easily interact with consumable-related data.
/// </summary>
public class ConsumableViewModel : ReactiveObject, IBuildComponentProvider
{
    private readonly ILogger<ConsumableViewModel> logger;
    private readonly Ship ship;

    public ConsumableViewModel()
        : this(new(), NullLogger<ConsumableViewModel>.Instance)
    {
    }

    private ConsumableViewModel(Ship ship, ILogger<ConsumableViewModel> logger)
    {
        this.ship = ship;
        this.logger = logger;
        ConsumableSlots = new();
    }

    /// <summary>
    /// Gets an ObservableCollection containing the slot numbers of all activated consumable slots.
    /// </summary>
    public ObservableCollection<int> ActivatedSlots { get; } = new();

    /// <summary>
    /// Gets an ObservableCollection containing a viewmodel for each consumable slot.
    /// </summary>
    public ObservableCollection<ConsumableSlotViewModel> ConsumableSlots { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="ConsumableViewModel"/> and initializes its data through asynchronous methods.
    /// </summary>
    /// <param name="ship">The ship associated with the new viewmodel instance.</param>
    /// <param name="disabledConsumables">A list of consumables that are currently disabled.</param>
    /// <param name="loggerFactory">The logger factory used to create a logger for the viewmodel.</param>
    /// <returns>A new instance of the <see cref="ConsumableViewModel"/> with initialized data.</returns>
    public static ConsumableViewModel Create(Ship ship, IEnumerable<string> disabledConsumables, ILoggerFactory loggerFactory)
    {
        var vm = new ConsumableViewModel(ship, loggerFactory.CreateLogger<ConsumableViewModel>());
        vm.UpdateSlotViewModels(disabledConsumables);
        return vm;
    }

    public void LoadBuild(IEnumerable<string> storedData)
    {
        foreach (var slotViewModel in ConsumableSlots)
        {
            var index = slotViewModel.ConsumableData.FindIndex(consumable => storedData.Any(s => consumable.IconName.StartsWith(s)));
            if (index > -1)
            {
                slotViewModel.SelectedIndex = index;
            }
        }
    }

    public List<string> SaveBuild()
    {
        return ConsumableSlots.Select(slot => slot.SelectedConsumable.IconName).ToList();
    }

    /// <summary>
    /// Updates the consumable data for each slot.
    /// This method only replaces the <see cref="ConsumableDataContainer">ConsumableDataContainer</see> objects in each slot viewmodel but not the viewmodels themselves.
    /// </summary>
    /// <param name="modifiers">The list of modifiers applied to the current ship.</param>
    /// <param name="shipHp">The HP of the ship after modifiers have been applied.</param>
    public void UpdateConsumableData(List<(string, float)> modifiers, int shipHp)
    {
        Parallel.ForEach(ConsumableSlots, consumableSlot => consumableSlot.UpdateDataContainers(modifiers, shipHp));
    }

    public IEnumerable<(string, float)> GetModifiersList()
    {
        var modifiers = new List<(string, float)>();
        foreach (int slot in ActivatedSlots)
        {
            var consumable = ConsumableSlots[slot].SelectedConsumable;
            if (consumable.Name.Contains("PCY015"))
            {
                modifiers.AddRange(consumable.Modifiers.Select(entry => ("speedBoost_" + entry.Key, entry.Value)));
            }
            else if (consumable.Name.Contains("PCY010"))
            {
                modifiers.AddRange(consumable.Modifiers.Select(entry => ("heal_" + entry.Key, entry.Value)));
            }
            else
            {
                modifiers.AddRange(consumable.Modifiers.Select(entry => (entry.Key, entry.Value)));
            }
        }

        return modifiers;
    }

    private void UpdateSlotViewModels(IEnumerable<string> disabledConsumables)
    {
        var rawSlots = ship.ShipConsumable.GroupBy(consumable => consumable.Slot)
            .Select(group => group.Where(c => !disabledConsumables.Contains(c.ConsumableName)))
            .Where(consumables => consumables.Any())
            .Select(consumables => ConsumableSlotViewModel.Create(consumables, Logging.LoggerFactory, ConsumableActivationChanged))
            .OrderBy(vm => vm.Slot);

        ConsumableSlots.Clear();
        ConsumableSlots.AddRange(rawSlots);
    }

    private void ConsumableActivationChanged(int slot, bool activationState)
    {
        if (activationState)
        {
            logger.LogDebug("Consumable slot {Slot} activated", slot);
            ActivatedSlots.Add(slot);
        }
        else
        {
            logger.LogDebug("Consumable slot {Slot} deactivated", slot);
            ActivatedSlots.Remove(slot);
        }
    }
}
