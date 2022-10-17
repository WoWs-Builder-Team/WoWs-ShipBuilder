using System;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.ShipVm
{
    public class UpgradePanelViewModelBase : ViewModelBase, IBuildComponentProvider
    {
        public static readonly Modernization PlaceholderModernization = new() { Index = null!, Name = "PlaceholderMod" };

        private List<List<Modernization>> availableModernizationList = null!;

        public UpgradePanelViewModelBase(Ship ship, Dictionary<string, Modernization> upgradeData)
        {
            List<Modernization> filteredModernizations = upgradeData.Select(entry => entry.Value)
                .Where(m => !(m.BlacklistedShips?.Contains(ship.Name) ?? false))
                .Where(m => m.ShipLevel?.Contains(ship.Tier) ?? false)
                .Where(m => m.AllowedNations?.Contains(ship.ShipNation) ?? false)
                .Where(m => m.ShipClasses?.Contains(ship.ShipClass) ?? false)
                .Union(upgradeData.Select(entry => entry.Value).Where(m => m.AdditionalShips?.Contains(ship.Name) ?? false))
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

            AvailableModernizationList = groupedList;
            SelectedModernizationList = new(AvailableModernizationList.Select(list => list.First()).Where(m => m.Index != null));

            OnModernizationSelected = (modernization, modernizationList) =>
            {
                int listIndex = AvailableModernizationList.IndexOf(modernizationList);
                var oldSelection = SelectedModernizationList.ToList().Find(m => AvailableModernizationList[listIndex].Contains(m));

                if (oldSelection != null)
                {
                    SelectedModernizationList.Remove(oldSelection);
                }

                if (modernization?.Index != null)
                {
                    SelectedModernizationList.Add(modernization);
                }

                this.RaisePropertyChanged(nameof(SelectedModernizationList));
            };
        }

        public Action<Modernization?, List<Modernization>> OnModernizationSelected { get; }

        public CustomObservableCollection<Modernization> SelectedModernizationList { get; }

        public List<List<Modernization>> AvailableModernizationList
        {
            get => availableModernizationList;
            set => this.RaiseAndSetIfChanged(ref availableModernizationList, value);
        }

        public List<(string, float)> GetModifierList()
        {
            return SelectedModernizationList
                .Where(m => m.Index != null)
                .SelectMany(m => m.Effect.Select(effect => (effect.Key, (float)effect.Value)))
                .ToList();
        }

        public void LoadBuild(IEnumerable<string> storedData)
        {
            var selection = new List<Modernization>();
            foreach (List<Modernization> modernizations in AvailableModernizationList)
            {
                selection.AddRange(modernizations.Where(modernization => storedData.Contains(modernization.Index)));
            }

            var removeList = SelectedModernizationList.Where(selected => selection.Any(newSelected => newSelected.Slot == selected.Slot)).ToList();
            SelectedModernizationList.RemoveMany(removeList);
            SelectedModernizationList.AddRange(selection);
            this.RaisePropertyChanged(nameof(SelectedModernizationList));
        }

        public List<string> SaveBuild()
        {
            return SelectedModernizationList.Select(modernization => modernization.Index).ToList();
        }
    }
}
