using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm;

/// <summary>
/// Represents the consumable slots of a ship.
/// Data for each slot is stored in an individual <see cref="ConsumableSlotViewModel"/>
/// while this viewmodel is responsible to store these individual instances and provide methods to easily interact with consumable-related data.
/// </summary>
public class ConsumableViewModel : ViewModelBase, IBuildComponentProvider
{
    private readonly IAppDataService appDataService;
    private readonly Ship ship;

    public ConsumableViewModel()
        : this(DesktopAppDataService.PreviewInstance, new())
    {
    }

    private ConsumableViewModel(IAppDataService appDataService, Ship ship)
    {
        this.appDataService = appDataService;
        this.ship = ship;
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
    /// Creates a new instance of the <see cref="ConsumableViewModel"/> and initializes its data through asynchronous methods.
    /// </summary>
    /// <param name="appDataService">The <see cref="IAppDataService"/> used by the new viewmodel.</param>
    /// <param name="ship">The ship associated with the new viewmodel instance.</param>
    /// <param name="disabledConsumables">A list of consumables that are currently disabled.</param>
    /// <returns>A new instance of the <see cref="ConsumableViewModel"/> with initialized data.</returns>
    public static async Task<ConsumableViewModel> CreateAsync(IAppDataService appDataService, Ship ship, IEnumerable<string> disabledConsumables)
    {
        var vm = new ConsumableViewModel(appDataService, ship);
        await vm.UpdateSlotViewModels(disabledConsumables);
        return vm;
    }

    /// <summary>
    /// Updates the consumable data for each slot.
    /// This method only replaces the <see cref="WoWsShipBuilder.Core.DataContainers.ConsumableDataContainer">ConsumableDataContainer</see> objects in each slot viewmodel but not the viewmodels themselves.
    /// </summary>
    /// <param name="modifiers">The list of modifiers applied to the current ship.</param>
    /// <param name="shipHp">The HP of the ship after modifiers have been applied.</param>
    public async Task UpdateConsumableData(List<(string, float)> modifiers, int shipHp)
    {
        await Parallel.ForEachAsync(ConsumableSlots, async (consumableSlot, _) => { await consumableSlot.UpdateDataContainers(modifiers, shipHp); });
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
            else
            {
                modifiers.AddRange(consumable.Modifiers.Select(entry => (entry.Key, entry.Value)));
            }
        }

        return modifiers;
    }

    private async Task UpdateSlotViewModels(IEnumerable<string> disabledConsumables)
    {
        var rawSlots = ship.ShipConsumable.GroupBy(consumable => consumable.Slot)
            .AsParallel()
            .Select(group => group.Where(c => !disabledConsumables.Contains(c.ConsumableName)))
            .Where(consumables => consumables.Any());
        var slots = new ConcurrentBag<ConsumableSlotViewModel>();

        await Parallel.ForEachAsync(rawSlots, async (consumables, _) => { slots.Add(await ConsumableSlotViewModel.CreateAsync(appDataService, consumables, ConsumableActivationChanged)); });

        ConsumableSlots.Clear();
        ConsumableSlots.AddRange(slots.OrderBy(vm => vm.Slot));
    }

    private void ConsumableActivationChanged(int slot, bool activationState)
    {
        if (activationState)
        {
            ActivatedSlots.Add(slot);
        }
        else
        {
            ActivatedSlots.Remove(slot);
        }
    }
}
