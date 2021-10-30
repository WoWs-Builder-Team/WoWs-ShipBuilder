using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class UpgradePanelViewModel : ViewModelBase
    {
        private List<List<Modernization>> availableModernizationList = null!;

        public UpgradePanelViewModel()
            : this(new Ship
            {
                Index = "PGSD109",
                Name = "PGSD109_Z_46",
                ShipClass = ShipClass.Destroyer,
                Tier = 9,
                ShipNation = Nation.Germany,
            })
        {
        }

        public UpgradePanelViewModel(Ship ship)
        {
            ModernizationChangedCallback = (modernization, modernizationList) =>
            {
                int listIndex = AvailableModernizationList.IndexOf(modernizationList);
                Modernization? oldSelection = SelectedModernizationList.ToList().Find(m => AvailableModernizationList[listIndex].Contains(m));
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

            Dictionary<string, Modernization> upgradeData = AppDataHelper.Instance.ReadLocalJsonData<Modernization>(Nation.Common, ServerType.Live) ??
                                                            new Dictionary<string, Modernization>();

            List<Modernization> filteredModernizations = upgradeData.Select(entry => entry.Value)
                .Where(m => !m.BlacklistedShips?.Contains(ship.Name) ?? false)
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
                subList.Insert(0, DataHelper.PlaceholderModernization);
            }

            AvailableModernizationList = groupedList;
            SelectedModernizationList = new AvaloniaList<Modernization>(AvailableModernizationList.Select(list => list.First()).Where(m => m.Index != null));
        }

        public Action<Modernization?, List<Modernization>> ModernizationChangedCallback { get; }

        public AvaloniaList<Modernization> SelectedModernizationList { get; }

        public List<List<Modernization>> AvailableModernizationList
        {
            get => availableModernizationList;
            set => this.RaiseAndSetIfChanged(ref availableModernizationList, value);
        }
    }
}
