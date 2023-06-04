using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels
{
    public class ShipModuleViewModel : ReactiveObject, IBuildComponentProvider
    {
        #region Static Fields and Constants

        private static readonly UpgradeInfo TestUpgradeInfo = new()
        {
            ShipUpgrades = new()
            {
                new() { UcType = ComponentType.Artillery, Name = "3", Prev = "2" },
                new() { UcType = ComponentType.Artillery, Name = "1", Prev = "" },
                new() { UcType = ComponentType.Artillery, Name = "2", Prev = "1" },
                new() { UcType = ComponentType.Hull, Name = "2", Prev = "1" },
                new() { UcType = ComponentType.Hull, Name = "1", Prev = "" },
            },
        };

        #endregion

        private List<List<ShipUpgrade>> shipUpgrades = null!;

        public ShipModuleViewModel()
            : this(TestUpgradeInfo)
        {
        }

        public ShipModuleViewModel(UpgradeInfo upgradeInfo)
        {
            ShipUpgrades = ShipModuleHelper.GroupAndSortUpgrades(upgradeInfo.ShipUpgrades).OrderBy(entry => entry.Key).Select(entry => entry.Value).ToList();
            foreach (List<ShipUpgrade> upgrade in ShipUpgrades)
            {
                SelectedModules.Add(upgrade.First());
            }
        }

        public List<List<ShipUpgrade>> ShipUpgrades
        {
            get => shipUpgrades;
            set
            {
                shipUpgrades = value;
                this.RaisePropertyChanged();
            }
        }

        public CustomObservableCollection<ShipUpgrade> SelectedModules { get; } = new();

        public void SelectModuleExecute(ShipUpgrade parameter)
        {
            if (SelectedModules.Contains(parameter))
            {
                return;
            }

            ShipUpgrade? oldItem = SelectedModules.FirstOrDefault(module => module.UcType == parameter.UcType);
            if (oldItem != null)
            {
                // SelectedModules.Remove(oldItem);
                SelectedModules.Replace(oldItem, parameter);
            }
            else
            {
                SelectedModules.Add(parameter);
            }
        }

        public void LoadBuild(IEnumerable<string> storedData)
        {
            var results = new List<ShipUpgrade>();
            foreach (List<ShipUpgrade> upgradeList in ShipUpgrades)
            {
                results.AddRange(upgradeList.Where(upgrade => storedData.Contains(upgrade.Name.NameToIndex())));
            }

            var modulesToRemove = SelectedModules.Where(module => results.Any(newSelection => newSelection.UcType == module.UcType)).ToList();
            SelectedModules.RemoveMany(modulesToRemove);
            SelectedModules.AddRange(results);
        }

        public List<string> SaveBuild()
        {
            return SelectedModules.Select(upgrade => upgrade.Name.NameToIndex()).ToList();
        }
    }
}
