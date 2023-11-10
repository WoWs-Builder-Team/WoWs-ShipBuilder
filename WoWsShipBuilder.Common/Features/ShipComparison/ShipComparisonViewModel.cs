using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.DataContainers;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Features.ShipComparison.GridData;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.DataTransfer;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipComparison;

public partial class ShipComparisonViewModel : ReactiveObject
{
    public const string DataNotAvailable = "N/A";

    private readonly AppSettings appSettings;

    private readonly IEnumerable<Ship> fullShipList = AppData.ShipDictionary.Values;

    private readonly ILocalizer localizer;

    private readonly Dictionary<Guid, GridDataWrapper> wrappersCache = new();

    private Dictionary<Guid, GridDataWrapper> filteredShipList = new();

    [Observable]
    private bool hideShipsWithoutSelectedSection;

    [Observable]
    private bool pinAllShips;

    private string researchedShip = string.Empty;

    [Observable]
    private string searchString = default!;

    [Observable]
    private bool selectAllShips;

    [Observable]
    private bool showPinnedShipsOnly;

    [Observable]
    private bool useUpgradedModules;

    public ShipComparisonViewModel(ILocalizer localizer, AppSettings appSettings)
    {
        this.localizer = localizer;
        this.appSettings = appSettings;

        this.useUpgradedModules = appSettings.ShipComparisonUseUpgradedModules;
        this.hideShipsWithoutSelectedSection = appSettings.ShipComparisonHideShipsWithoutSection;
        this.Range = appSettings.ShipComparisonFiringRange;
    }

    private Dictionary<Guid, GridDataWrapper> FilteredShipList
    {
        get => this.filteredShipList;
        set => this.RaiseAndSetIfChanged(ref this.filteredShipList, value);
    }

    public Dictionary<Guid, GridDataWrapper> SelectedShipList { get; } = new();

    public Dictionary<Guid, GridDataWrapper> PinnedShipList { get; } = new();

    public List<ShipComparisonDataSections> DataSections { get; private set; } = new() { ShipComparisonDataSections.General };

    public ShipComparisonDataSections SelectedDataSection { get; set; } = ShipComparisonDataSections.General;

    public ObservableCollection<int> SelectedTiers { get; } = new();

    public ObservableCollection<ShipClass> SelectedClasses { get; } = new();

    public ObservableCollection<Nation> SelectedNations { get; } = new();

    public ObservableCollection<ShipCategory> SelectedCategories { get; } = new();

    public IEnumerable<ShipClass> AvailableClasses { get; } = Enum.GetValues<ShipClass>().Except(new[] { ShipClass.Auxiliary });

    public IEnumerable<Nation> AvailableNations { get; } = Enum.GetValues<Nation>().Except(new[] { Nation.Common });

    public IEnumerable<ShipCategory> AvailableShipCategories { get; } = Enum.GetValues<ShipCategory>().Except(new[] { ShipCategory.Disabled, ShipCategory.Clan });

    public List<Ship> SearchedShips { get; } = new();

    public Dictionary<Guid, DispersionContainer> DispersionCache { get; } = new();

    public double Range { get; private set; }

    public string ResearchedShip
    {
        get => this.researchedShip;
        set
        {
            this.researchedShip = value;
            this.FindShips();
            this.RaiseAndSetIfChanged(ref this.researchedShip, value);
        }
    }

    private static bool ContainsShipIndex(string shipIndex, IEnumerable<KeyValuePair<Guid, GridDataWrapper>> list)
    {
        return list.Select(x => x.Value.ShipIndex).Contains(shipIndex);
    }

    public Task ApplyFilters()
    {
        Dictionary<Guid, GridDataWrapper> dictionary = new();

        dictionary.AddRange(this.PinnedShipList);

        var filteredShips = this.FilteredShipList.Where(data => this.SelectedTiers.Contains(data.Value.Ship.Tier) && this.SelectedClasses.Contains(data.Value.Ship.ShipClass) && this.SelectedNations.Contains(data.Value.Ship.ShipNation) && this.SelectedCategories.Contains(data.Value.Ship.ShipCategory)).ToDictionary(x => x.Key, x => x.Value);

        dictionary.AddRange(filteredShips.Where(x => !dictionary.ContainsKey(x.Key)));

        var cachedWrappers = this.wrappersCache.Where(data => this.SelectedTiers.Contains(data.Value.Ship.Tier) && this.SelectedClasses.Contains(data.Value.Ship.ShipClass) && this.SelectedNations.Contains(data.Value.Ship.ShipNation) && this.SelectedCategories.Contains(data.Value.Ship.ShipCategory)).ToDictionary(x => x.Key, x => x.Value);

        dictionary.AddRange(cachedWrappers.Where(x => !dictionary.ContainsKey(x.Key)));

        dictionary.AddRange(this.InitialiseShipBuildContainers(this.fullShipList.Where(data => !ContainsShipIndex(data.Index, filteredShips) &&
                                                                                               !ContainsShipIndex(data.Index, cachedWrappers) && this.SelectedTiers.Contains(data.Tier) && this.SelectedClasses.Contains(data.ShipClass) && this.SelectedNations.Contains(data.ShipNation) && this.SelectedCategories.Contains(data.ShipCategory))));

        this.wrappersCache.RemoveMany(cachedWrappers);
        this.FilteredShipList.RemoveMany(filteredShips);
        this.SelectedShipList.RemoveMany(this.FilteredShipList.Where(x => this.SelectedShipList.ContainsKey(x.Key) && !this.PinnedShipList.ContainsKey(x.Key)));
        this.wrappersCache.AddRange(this.FilteredShipList.Where(x => !this.wrappersCache.ContainsKey(x.Key)));

        this.SetSelectAndPinAllButtonsStatus(dictionary);

        this.FilteredShipList = dictionary;

        this.GetDataSectionsToDisplay();

        // Keep the return type as task in order to allow making the method async in the future if there is heavy filtering logic that needs to run asynchronously.
        return Task.CompletedTask;
    }

    public void ToggleShowPinnedShipOnly()
    {
        this.ShowPinnedShipsOnly = !this.ShowPinnedShipsOnly;
        if (this.ShowPinnedShipsOnly)
        {
            this.PinAllShips = true;
            this.SelectAllShips = this.FilteredShipList.Intersect(this.PinnedShipList).All(x => this.SelectedShipList.ContainsKey(x.Key));
        }
        else
        {
            this.SetSelectAndPinAllButtonsStatus();
        }
    }

    public async Task ToggleTierSelection(int value)
    {
        if (this.SelectedTiers.Contains(value))
        {
            this.SelectedTiers.Remove(value);
        }
        else
        {
            this.SelectedTiers.Add(value);
        }

        await this.ApplyFilters();
    }

    public async Task ToggleClassSelection(ShipClass value)
    {
        if (this.SelectedClasses.Contains(value))
        {
            this.SelectedClasses.Remove(value);
        }
        else
        {
            this.SelectedClasses.Add(value);
        }

        await this.ApplyFilters();
    }

    public async Task ToggleNationSelection(Nation value)
    {
        if (this.SelectedNations.Contains(value))
        {
            this.SelectedNations.Remove(value);
        }
        else
        {
            this.SelectedNations.Add(value);
        }

        await this.ApplyFilters();
    }

    public async Task ToggleCategorySelection(ShipCategory value)
    {
        if (this.SelectedCategories.Contains(value))
        {
            this.SelectedCategories.Remove(value);
        }
        else
        {
            this.SelectedCategories.Add(value);
        }

        await this.ApplyFilters();
    }

    public async Task ToggleAllTiers(bool activationState, bool applyFilters = true)
    {
        this.SelectedTiers.Clear();

        if (activationState)
        {
            this.SelectedTiers.AddRange(Enumerable.Range(1, 11));
        }

        if (applyFilters)
        {
            await this.ApplyFilters();
        }
    }

    public async Task ToggleAllClasses(bool activationState, bool applyFilters = true)
    {
        this.SelectedClasses.Clear();

        if (activationState)
        {
            this.SelectedClasses.AddRange(Enum.GetValues<ShipClass>().Except(new[] { ShipClass.Auxiliary }));
        }

        if (applyFilters)
        {
            await this.ApplyFilters();
        }
    }

    public async Task ToggleAllNations(bool activationState, bool applyFilters = true)
    {
        this.SelectedNations.Clear();

        if (activationState)
        {
            this.SelectedNations.AddRange(Enum.GetValues<Nation>().Except(new[] { Nation.Common }));
        }

        if (applyFilters)
        {
            await this.ApplyFilters();
        }
    }

    public async Task ToggleAllCategories(bool activationState, bool applyFilters = true)
    {
        this.SelectedCategories.Clear();

        if (activationState)
        {
            this.SelectedCategories.AddRange(Enum.GetValues<ShipCategory>().Except(new[] { ShipCategory.Disabled, ShipCategory.Clan }));
        }

        if (applyFilters)
        {
            await this.ApplyFilters();
        }
    }

    public Dictionary<Guid, GridDataWrapper> GetShipsToBeDisplayed()
    {
        return this.GetShipsToBeDisplayed(false);
    }

    public void EditBuilds(IEnumerable<KeyValuePair<Guid, GridDataWrapper>> newWrappers)
    {
        this.EditBuilds(newWrappers, false);
    }

    public Dictionary<Guid, GridDataWrapper> RemoveBuilds(IEnumerable<KeyValuePair<Guid, GridDataWrapper>> wrappers)
    {
        Dictionary<Guid, GridDataWrapper> warnings = new();
        var buildList = wrappers.ToDictionary(x => x.Key, x => x.Value);
        foreach (var wrapper in buildList)
        {
            if (this.FilteredShipList.Count(x => x.Value.Ship.Index.Equals(wrapper.Value.Ship.Index)) > 1)
            {
                this.FilteredShipList.Remove(wrapper.Key);

                this.PinnedShipList.Remove(wrapper.Key);

                if (this.wrappersCache.ContainsKey(wrapper.Key))
                {
                    this.wrappersCache[wrapper.Key] = wrapper.Value;
                }

                this.DispersionCache.Remove(wrapper.Key);
            }
            else
            {
                if (wrapper.Value.Build is not null)
                {
                    var err = this.ResetBuild(wrapper.Value);
                    this.EditBuilds(new Dictionary<Guid, GridDataWrapper> { { err.Id, err } });
                    warnings.Add(err.Id, err);
                }
                else
                {
                    warnings.Add(wrapper.Key, wrapper.Value);
                }
            }
        }

        this.SelectedShipList.Clear();
        this.SelectedShipList.AddRange(warnings);

        this.SetSelectAndPinAllButtonsStatus();

        return warnings;
    }

    public void ResetAllBuilds()
    {
        this.RemoveBuilds(this.FilteredShipList);
        this.SelectedShipList.Clear();
        var list = this.FilteredShipList.Where(x => x.Value.Build is not null).ToDictionary(x => x.Key, x => this.ResetBuild(x.Value));
        this.EditBuilds(list, true);
        this.SetSelectAndPinAllButtonsStatus();
    }

    public async Task AddPinnedShip(GridDataWrapper wrapper)
    {
        if (!this.PinnedShipList.ContainsKey(wrapper.Id))
        {
            this.PinnedShipList.Add(wrapper.Id, wrapper);
        }
        else
        {
            await this.RemovePinnedShip(wrapper);
        }

        this.SetSelectAndPinAllButtonsStatus();
    }

    public void AddSelectedShip(GridDataWrapper wrapper)
    {
        if (!this.SelectedShipList.ContainsKey(wrapper.Id))
        {
            this.SelectedShipList.Add(wrapper.Id, wrapper);
        }
        else
        {
            this.RemoveSelectedShip(wrapper);
        }

        this.SetSelectAndPinAllButtonsStatus();
    }

    public void ToggleUpgradedModules()
    {
        this.UseUpgradedModules = !this.UseUpgradedModules;
        this.ChangeModulesBatch();
    }

    public void ToggleHideShipsWithoutSelectedSection()
    {
        this.HideShipsWithoutSelectedSection = !this.HideShipsWithoutSelectedSection;
    }

    public async Task SelectDataSection(ShipComparisonDataSections dataSection)
    {
        this.SelectedDataSection = dataSection;
        await this.ApplyFilters();
    }

    public void SelectAllDisplayedShips()
    {
        this.SelectAllShips = !this.SelectAllShips;

        var list = this.GetShipsToBeDisplayed();

        if (!string.IsNullOrEmpty(this.searchString))
        {
            list = list.Where(x => this.appSettings.SelectedLanguage.CultureInfo.CompareInfo.IndexOf(this.localizer.GetGameLocalization(x.Value.Ship.Index + "_FULL").Localization, this.searchString, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1).ToDictionary(x => x.Key, x => x.Value);
        }

        if (this.SelectAllShips)
        {
            this.SelectedShipList.AddRange(list.Where(x => !this.SelectedShipList.ContainsKey(x.Key)));
        }
        else
        {
            this.SelectedShipList.RemoveMany(list.Where(x => this.SelectedShipList.ContainsKey(x.Key)));
        }
    }

    public async Task PinAllDisplayedShips()
    {
        this.PinAllShips = !this.PinAllShips;

        var list = this.FilteredShipList;

        if (!string.IsNullOrEmpty(this.searchString))
        {
            list = list.Where(x => this.appSettings.SelectedLanguage.CultureInfo.CompareInfo.IndexOf(this.localizer.GetGameLocalization(x.Value.Ship.Index + "_FULL").Localization, this.searchString, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1).ToDictionary(x => x.Key, x => x.Value);
        }

        if (this.PinAllShips)
        {
            this.PinnedShipList.AddRange(list.Where(x => !this.PinnedShipList.ContainsKey(x.Key)));
        }
        else
        {
            this.PinnedShipList.RemoveMany(list.Where(x => this.PinnedShipList.ContainsKey(x.Key)));
            await this.ApplyFilters();
        }
    }

    public void DuplicateSelectedShips()
    {
        foreach (var selectedShip in this.SelectedShipList)
        {
            var newWrapper = new GridDataWrapper(selectedShip.Value.ShipBuildContainer with { Id = Guid.NewGuid() });
            this.FilteredShipList.Add(newWrapper.Id, newWrapper);
            if (this.PinnedShipList.ContainsKey(selectedShip.Key))
            {
                this.PinnedShipList.Add(newWrapper.Id, newWrapper);
            }

            if (this.DispersionCache.ContainsKey(selectedShip.Key))
            {
                this.DispersionCache[newWrapper.Id] = this.DispersionCache[selectedShip.Key];
            }
        }

        this.SetSelectAndPinAllButtonsStatus();
    }

    public void AddShip(object? obj)
    {
        GridDataWrapper newWrapper;

        if (obj is GridDataWrapper wrapper)
        {
            newWrapper = wrapper;
        }
        else if (obj is ShipBuildContainer container)
        {
            newWrapper = new(container);
        }
        else if (obj is Ship ship)
        {
            newWrapper = new(ShipBuildContainer.CreateNew(ship, null, null) with { ShipDataContainer = this.GetShipDataContainer(ship) });
        }
        else if (obj is string shipIndex)
        {
            var shipFromIndex = this.fullShipList.First(x => x.Index.Equals(shipIndex, StringComparison.Ordinal));
            newWrapper = new(ShipBuildContainer.CreateNew(shipFromIndex, null, null) with { ShipDataContainer = this.GetShipDataContainer(shipFromIndex) });
        }
        else
        {
            return;
        }

        this.FilteredShipList.Add(newWrapper.Id, newWrapper);
        this.PinnedShipList.Add(newWrapper.Id, newWrapper);

        this.SetSelectAndPinAllButtonsStatus();
        this.GetDataSectionsToDisplay();
        this.SearchString = string.Empty;
        this.ResearchedShip = string.Empty;
        this.SearchedShips.Clear();
    }

    public void UpdateRange(double selectedValue)
    {
        this.Range = selectedValue;
    }

    private Dictionary<Guid, GridDataWrapper> GetShipsToBeDisplayed(bool disableHideShipsIfNoSelectedSection)
    {
        Dictionary<Guid, GridDataWrapper> list = this.ShowPinnedShipsOnly ? this.PinnedShipList : this.FilteredShipList;

        if (!disableHideShipsIfNoSelectedSection)
        {
            list = this.HideShipsIfNoSelectedSection(list);
        }

        this.CacheDispersion(list.Values);

        return list;
    }

    private void EditBuilds(IEnumerable<KeyValuePair<Guid, GridDataWrapper>> newWrappers, bool clearCache)
    {
        foreach (var wrapper in newWrappers)
        {
            this.FilteredShipList[wrapper.Key] = wrapper.Value;

            if (this.PinnedShipList.ContainsKey(wrapper.Key))
            {
                this.PinnedShipList[wrapper.Key] = wrapper.Value;
            }
            else if (ContainsShipIndex(wrapper.Value.Ship.Index, this.PinnedShipList))
            {
                this.PinnedShipList.Add(wrapper.Key, wrapper.Value);
            }

            if (!clearCache && this.wrappersCache.ContainsKey(wrapper.Key))
            {
                this.wrappersCache[wrapper.Key] = wrapper.Value;
            }

            if (this.SelectedShipList.ContainsKey(wrapper.Key))
            {
                this.SelectedShipList[wrapper.Key] = wrapper.Value;
            }
        }

        if (clearCache)
        {
            this.wrappersCache.Clear();
        }
    }

    private async Task RemovePinnedShip(GridDataWrapper wrapper)
    {
        this.PinnedShipList.Remove(wrapper.Id);
        await this.ApplyFilters();
    }

    private void RemoveSelectedShip(GridDataWrapper wrapper)
    {
        this.SelectedShipList.Remove(wrapper.Id);
    }

    private Dictionary<Guid, GridDataWrapper> InitialiseShipBuildContainers(IEnumerable<Ship> ships)
    {
        return ships.Select(ship => new GridDataWrapper(ShipBuildContainer.CreateNew(ship, null, null) with { ShipDataContainer = this.GetShipDataContainer(ship) })).ToDictionary(x => x.Id, x => x);
    }

    private void ChangeModulesBatch()
    {
        this.EditBuilds(this.FilteredShipList.Where(x => x.Value.Build is null).ToDictionary(x => x.Key, x => this.ResetBuild(x.Value)));
    }

    private List<ShipUpgrade> GetShipConfiguration(Ship ship)
    {
        List<ShipUpgrade> shipConfiguration = this.UseUpgradedModules
            ? ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
                .OrderBy(entry => entry.Key)
                .Select(entry => entry.Value)
                .Select(module => module[^1])
                .ToList()
            : ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
                .OrderBy(entry => entry.Key)
                .Select(entry => entry.Value)
                .Select(module => module[0])
                .ToList();
        return shipConfiguration;
    }

    private ShipDataContainer GetShipDataContainer(Ship ship)
    {
        return ShipDataContainer.CreateFromShip(ship, this.GetShipConfiguration(ship), new());
    }

    private Dictionary<Guid, GridDataWrapper> HideShipsIfNoSelectedSection(IEnumerable<KeyValuePair<Guid, GridDataWrapper>> list)
    {
        if (!this.hideShipsWithoutSelectedSection)
        {
            return list.ToDictionary(x => x.Key, x => x.Value);
        }

        Dictionary<Guid, GridDataWrapper> newList = this.SelectedDataSection switch
        {
            ShipComparisonDataSections.MainBattery => list.Where(x => x.Value.ShipDataContainer.MainBatteryDataContainer is not null).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.He => list.Where(x => x.Value.HeShell?.Damage is not null).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.Ap => list.Where(x => x.Value.ApShell?.Damage is not null).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.Sap => list.Where(x => x.Value.SapShell?.Damage is not null).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.Torpedo => list.Where(x => x.Value.ShipDataContainer.TorpedoArmamentDataContainer is not null).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.RocketPlanes => list.Where(x => x.Value.RocketPlanes.Type.Any()).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.Rockets => list.Where(x => x.Value.RocketPlanes.WeaponType.Any()).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.TorpedoBombers => list.Where(x => x.Value.TorpedoBombers.Type.Any()).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.AerialTorpedoes => list.Where(x => x.Value.TorpedoBombers.WeaponType.Any()).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.Bombers => list.Where(x => x.Value.Bombers.Type.Any()).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.Bombs => list.Where(x => x.Value.Bombers.WeaponType.Any()).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.Sonar => list.Where(x => x.Value.ShipDataContainer.PingerGunDataContainer is not null).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.SecondaryBattery => list.Where(x => x.Value.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries is not null).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.SecondaryBatteryShells => list.Where(x => x.Value.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries is not null).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.AntiAir => list.Where(x => x.Value.ShipDataContainer.AntiAirDataContainer is not null).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.AirStrike => list.Where(x => x.Value.ShipDataContainer.AirstrikeDataContainer is not null).ToDictionary(x => x.Key, x => x.Value),
            ShipComparisonDataSections.Asw => list.Where(x => x.Value.ShipDataContainer.AswAirstrikeDataContainer is not null || x.Value.ShipDataContainer.DepthChargeLauncherDataContainer is not null).ToDictionary(x => x.Key, x => x.Value),
            _ => list.ToDictionary(x => x.Key, x => x.Value),
        };

        newList.AddRange(this.PinnedShipList.Where(x => !newList.ContainsKey(x.Key)));
        return newList;
    }

    private void GetDataSectionsToDisplay()
    {
        var displayedShipList = this.GetShipsToBeDisplayed(true);
        this.DataSections = !displayedShipList.Any() ? new() { ShipComparisonDataSections.General } : this.HideEmptyDataSections(displayedShipList);
    }

    [SuppressMessage("Performance", "CA1822", Justification = "not static to preserve file structure")]
    private List<ShipComparisonDataSections> HideEmptyDataSections(Dictionary<Guid, GridDataWrapper> displayedShips)
    {
        var dataSections = Enum.GetValues<ShipComparisonDataSections>().ToList();
        foreach (var dataSection in Enum.GetValues<ShipComparisonDataSections>().Except(new[] { ShipComparisonDataSections.Maneuverability, ShipComparisonDataSections.Concealment, ShipComparisonDataSections.Survivability, ShipComparisonDataSections.General }))
        {
            switch (dataSection)
            {
                case ShipComparisonDataSections.MainBattery when !displayedShips.Any(x => x.Value.ShipDataContainer.MainBatteryDataContainer is not null):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.He when !displayedShips.Any(x => x.Value.HeShell?.Damage is not null):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.Ap when !displayedShips.Any(x => x.Value.ApShell?.Damage is not null):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.Sap when !displayedShips.Any(x => x.Value.SapShell?.Damage is not null):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.Torpedo when !displayedShips.Any(x => x.Value.ShipDataContainer.TorpedoArmamentDataContainer is not null):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.SecondaryBattery when !displayedShips.Any(x => x.Value.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries is not null):
                case ShipComparisonDataSections.SecondaryBatteryShells when !displayedShips.Any(x => x.Value.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries is not null):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.AntiAir when !displayedShips.Any(x => x.Value.ShipDataContainer.AntiAirDataContainer is not null):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.Asw when !displayedShips.Any(x => x.Value.ShipDataContainer.AswAirstrikeDataContainer is not null || x.Value.ShipDataContainer.DepthChargeLauncherDataContainer is not null):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.AirStrike when !displayedShips.Any(x => x.Value.ShipDataContainer.AirstrikeDataContainer is not null):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.RocketPlanes when !displayedShips.Any(x => x.Value.RocketPlanes.Type.Any()):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.Rockets when !displayedShips.Any(x => x.Value.RocketPlanes.WeaponType.Any()):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.TorpedoBombers when !displayedShips.Any(x => x.Value.TorpedoBombers.Type.Any()):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.AerialTorpedoes when !displayedShips.Any(x => x.Value.TorpedoBombers.WeaponType.Any()):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.Bombers when !displayedShips.Any(x => x.Value.Bombers.Type.Any()):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.Bombs when !displayedShips.Any(x => x.Value.Bombers.WeaponType.Any()):
                    dataSections.Remove(dataSection);
                    break;
                case ShipComparisonDataSections.Sonar when !displayedShips.Any(x => x.Value.ShipDataContainer.PingerGunDataContainer is not null):
                    dataSections.Remove(dataSection);
                    break;
            }
        }

        return dataSections;
    }

    private void FindShips()
    {
        if (string.IsNullOrEmpty(this.researchedShip.Trim()))
        {
            return;
        }

        this.SearchedShips.Clear();
        this.SearchedShips.AddRange(this.fullShipList.Where(ship => this.appSettings.SelectedLanguage.CultureInfo.CompareInfo.IndexOf(this.localizer.GetGameLocalization(ship.Index + "_FULL").Localization, this.researchedShip, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1));
    }

    private void SetSelectAndPinAllButtonsStatus() => this.SetSelectAndPinAllButtonsStatus(this.GetShipsToBeDisplayed());

    private void SetSelectAndPinAllButtonsStatus(IReadOnlyDictionary<Guid, GridDataWrapper> list)
    {
        this.SelectAllShips = list.All(wrapper => this.SelectedShipList.ContainsKey(wrapper.Key));
        this.PinAllShips = list.All(wrapper => this.PinnedShipList.ContainsKey(wrapper.Key));
    }

    private GridDataWrapper ResetBuild(GridDataWrapper wrapper)
    {
        GridDataWrapper reset = new(wrapper.ShipBuildContainer with { Build = null, ActivatedConsumableSlots = null, SpecialAbilityActive = false, ShipDataContainer = this.GetShipDataContainer(wrapper.Ship), Modifiers = null });
        return reset;
    }

    // this is needed because dispersion is not calculated inside the GridDataWrapper in order to not recalculate oll of it upon each range variation
    private void CacheDispersion(IEnumerable<GridDataWrapper> wrappers)
    {
        foreach (var wrapper in wrappers)
        {
            if (wrapper.MainBattery?.DispersionData is not null && wrapper.MainBattery?.DispersionModifier is not null && wrapper.MainBattery?.Range is not null)
            {
                this.DispersionCache[wrapper.Id] = wrapper.MainBattery.DispersionData.CalculateDispersion(decimal.ToDouble(wrapper.MainBattery.Range * 1000), wrapper.MainBattery.DispersionModifier, this.Range * 1000);
            }
        }
    }
}
