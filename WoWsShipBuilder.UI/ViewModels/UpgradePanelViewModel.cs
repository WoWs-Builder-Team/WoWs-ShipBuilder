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
            : this(
                new Ship
                {
                    Index = "PGSD109",
                    Name = "PGSD109_Z_46",
                    ShipClass = ShipClass.Destroyer,
                    Tier = 9,
                }, Nation.Germany)
        {
        }

        public UpgradePanelViewModel(Ship ship, Nation shipNation)
        {
            ModernizationChangedCallback = (modernization, modernizationList) =>
            {
                int listIndex = AvailableModernizationList.IndexOf(modernizationList);
                Modernization? oldSelection = SelectedModernizationList.ToList().Find(m => AvailableModernizationList[listIndex].Contains(m));
                if (oldSelection != null)
                {
                    SelectedModernizationList.Remove(oldSelection);
                }

                if (modernization != null)
                {
                    SelectedModernizationList.Add(modernization);
                }
            };

            Dictionary<string, Modernization> upgradeData = AppDataHelper.Instance.ReadLocalJsonData<Modernization>(Nation.Common, ServerType.Live) ??
                                                            new Dictionary<string, Modernization>();

            var filteredModernizations = upgradeData.Select(entry => entry.Value)
                .Where(m => !m.BlacklistedShips.Contains(ship.Name))
                .Where(m => m.ShipLevel.Contains(ship.Tier))
                .Where(m => m.AllowedNations.Contains(shipNation))
                .Where(m => m.ShipClasses.Contains(ship.ShipClass))
                .Union(upgradeData.Select(entry => entry.Value).Where(m => m.AdditionalShips.Contains(ship.Name)))
                .ToList();

            List<List<Modernization>> groupedList = filteredModernizations.GroupBy(m => m.Slot)
                .Select(group => (group.Key, group.OrderBy(m => m.Index).ToList()))
                .OrderBy(item => item.Key)
                .Select(item => item.Item2)
                .ToList();

            AvailableModernizationList = groupedList;
        }

        public Action<Modernization?, List<Modernization>> ModernizationChangedCallback { get; }

        public AvaloniaList<Modernization> SelectedModernizationList { get; } = new();

        public List<List<Modernization>> AvailableModernizationList
        {
            get => availableModernizationList;
            set => this.RaiseAndSetIfChanged(ref availableModernizationList, value);
        }
    }
}
