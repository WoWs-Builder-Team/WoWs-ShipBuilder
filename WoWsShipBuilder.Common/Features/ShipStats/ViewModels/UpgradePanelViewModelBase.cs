using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public class UpgradePanelViewModelBase : ReactiveObject, IBuildComponentProvider
{
    public static readonly Modernization PlaceholderModernization = new() { Index = null!, Name = "PlaceholderMod" };

    private List<List<Modernization>> availableModernizationList = null!;

    public UpgradePanelViewModelBase(Ship ship, Dictionary<string, Modernization> upgradeData)
    {
        List<Modernization> filteredModernizations = upgradeData.Select(entry => entry.Value)
            .Where(m => !m.BlacklistedShips.Contains(ship.Name))
            .Where(m => m.ShipLevel.Contains(ship.Tier))
            .Where(m => ship.ShipNation == Nation.Common || m.AllowedNations.Contains(ship.ShipNation))
            .Where(m => m.ShipClasses.Contains(ship.ShipClass))
            .Union(upgradeData.Select(entry => entry.Value).Where(m => m.AdditionalShips.Contains(ship.Name)))
            .ToList();

        List<List<Modernization>> groupedList = filteredModernizations.GroupBy(m => m.Slot)
            .Select(group => (group.Key, group.OrderBy(m => m.Type).ThenBy(m => m.Index).ToList()))
            .OrderBy(item => item.Key)
            .Select(item => item.Item2)
            .ToList();

        foreach (List<Modernization> subList in groupedList)
        {
            subList.Insert(0, PlaceholderModernization);
        }

        this.AvailableModernizationList = groupedList;
        this.SelectedModernizationList = new(this.AvailableModernizationList.Select(list => list[0]).Where(m => !string.IsNullOrEmpty(m.Index)));

        this.OnModernizationSelected = (modernization, modernizationList) =>
        {
            int listIndex = this.AvailableModernizationList.IndexOf(modernizationList);
            var oldSelection = this.SelectedModernizationList.ToList().Find(m => this.AvailableModernizationList[listIndex].Contains(m));

            if (oldSelection != null)
            {
                this.SelectedModernizationList.Remove(oldSelection);
            }

            if (modernization?.Index != null)
            {
                this.SelectedModernizationList.Add(modernization);
            }

            this.RaisePropertyChanged(nameof(this.SelectedModernizationList));
        };
    }

    public Action<Modernization?, List<Modernization>> OnModernizationSelected { get; }

    public CustomObservableCollection<Modernization> SelectedModernizationList { get; }

    public List<List<Modernization>> AvailableModernizationList
    {
        get => this.availableModernizationList;
        set => this.RaiseAndSetIfChanged(ref this.availableModernizationList, value);
    }

    public List<(string, float)> GetModifierList()
    {
        var modifiers = this.SelectedModernizationList
            .Where(m => !string.IsNullOrEmpty(m.Index))
            .SelectMany(m => m.Effect.Select(effect => (effect.Key, (float)effect.Value)))
            .ToList();

        // modifiers.FindIndex(pair => pair.Key.Contains())
        return modifiers;
    }

    public void LoadBuild(IEnumerable<string> storedData)
    {
        var selection = new List<Modernization>();
        foreach (List<Modernization> modernizations in this.AvailableModernizationList)
        {
            selection.AddRange(modernizations.Where(modernization => storedData.Contains(modernization.Index)));
        }

        var removeList = this.SelectedModernizationList.Where(selected => selection.Exists(newSelected => newSelected.Slot == selected.Slot)).ToList();
        this.SelectedModernizationList.RemoveMany(removeList);
        this.SelectedModernizationList.AddRange(selection);
        this.RaisePropertyChanged(nameof(this.SelectedModernizationList));
    }

    public List<string> SaveBuild()
    {
        return this.SelectedModernizationList.Select(modernization => modernization.Index).ToList();
    }
}
