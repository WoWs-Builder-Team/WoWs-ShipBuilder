using System.Collections.Immutable;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels
{
    public class ShipModuleViewModel : ReactiveObject, IBuildComponentProvider
    {
        #region Static Fields and Constants

        private static readonly UpgradeInfo TestUpgradeInfo = new()
        {
            ShipUpgrades = ImmutableList.Create<ShipUpgrade>(
                new() { UcType = ComponentType.Artillery, Name = "3", Prev = "2" },
                new() { UcType = ComponentType.Artillery, Name = "1", Prev = "" },
                new() { UcType = ComponentType.Artillery, Name = "2", Prev = "1" },
                new() { UcType = ComponentType.Hull, Name = "2", Prev = "1" },
                new() { UcType = ComponentType.Hull, Name = "1", Prev = "" }),
        };

        #endregion

        private List<List<ShipUpgrade>> shipUpgrades = null!;

        // TODO: remove this constructor
        public ShipModuleViewModel()
            : this(TestUpgradeInfo)
        {
        }

        public ShipModuleViewModel(UpgradeInfo upgradeInfo)
        {
            this.ShipUpgrades = ShipModuleHelper.GroupAndSortUpgrades(upgradeInfo.ShipUpgrades).OrderBy(entry => entry.Key).Select(entry => entry.Value).ToList();
            foreach (List<ShipUpgrade> upgrade in this.ShipUpgrades)
            {
                this.SelectedModules.Add(upgrade[0]);
            }
        }

        public List<List<ShipUpgrade>> ShipUpgrades
        {
            get => this.shipUpgrades;
            set
            {
                this.shipUpgrades = value;
                this.RaisePropertyChanged();
            }
        }

        public CustomObservableCollection<ShipUpgrade> SelectedModules { get; } = new();

        public void SelectModuleExecute(ShipUpgrade parameter)
        {
            if (this.SelectedModules.Contains(parameter))
            {
                return;
            }

            ShipUpgrade? oldItem = this.SelectedModules.FirstOrDefault(module => module.UcType == parameter.UcType);
            if (oldItem != null)
            {
                this.SelectedModules.Replace(oldItem, parameter);
            }
            else
            {
                this.SelectedModules.Add(parameter);
            }
        }

        public void LoadBuild(IEnumerable<string> storedData)
        {
            var results = new List<ShipUpgrade>();
            foreach (List<ShipUpgrade> upgradeList in this.ShipUpgrades)
            {
                results.AddRange(upgradeList.Where(upgrade => storedData.Contains(upgrade.Name.NameToIndex())));
            }

            var modulesToRemove = this.SelectedModules.Where(module => results.Exists(newSelection => newSelection.UcType == module.UcType)).ToList();
            this.SelectedModules.RemoveMany(modulesToRemove);
            this.SelectedModules.AddRange(results);
        }

        public List<string> SaveBuild()
        {
            return this.SelectedModules.Select(upgrade => upgrade.Name.NameToIndex()).ToList();
        }
    }
}
