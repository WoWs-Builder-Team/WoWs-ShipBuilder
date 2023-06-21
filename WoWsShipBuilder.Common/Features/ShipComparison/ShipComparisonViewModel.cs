using System.Collections.ObjectModel;
using System.Globalization;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.DataContainers;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
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
    }

    private Dictionary<Guid, GridDataWrapper> FilteredShipList
    {
        get => filteredShipList;
        set => this.RaiseAndSetIfChanged(ref filteredShipList, value);
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

    public double Range { get; private set; } = 10;

    public string ResearchedShip
    {
        get => researchedShip;
        set
        {
            researchedShip = value;
            FindShips();
            this.RaiseAndSetIfChanged(ref researchedShip, value);
        }
    }

    private static bool ContainsShipIndex(string shipIndex, IEnumerable<KeyValuePair<Guid, GridDataWrapper>> list)
    {
        return list.Select(x => x.Value.ShipIndex).Contains(shipIndex);
    }

    public Task ApplyFilters()
    {
        Dictionary<Guid, GridDataWrapper> dictionary = new();

        dictionary.AddRange(PinnedShipList);

        var filteredShips = FilteredShipList.Where(data => SelectedTiers.Contains(data.Value.Ship.Tier) &&
                                                           SelectedClasses.Contains(data.Value.Ship.ShipClass) &&
                                                           SelectedNations.Contains(data.Value.Ship.ShipNation) &&
                                                           SelectedCategories.Contains(data.Value.Ship.ShipCategory)).ToDictionary(x => x.Key, x => x.Value);

        dictionary.AddRange(filteredShips.Where(x => !dictionary.ContainsKey(x.Key)));

        Dictionary<Guid, GridDataWrapper> cachedWrappers = wrappersCache.Where(data => SelectedTiers.Contains(data.Value.Ship.Tier) &&
                                                                                       SelectedClasses.Contains(data.Value.Ship.ShipClass) &&
                                                                                       SelectedNations.Contains(data.Value.Ship.ShipNation) &&
                                                                                       SelectedCategories.Contains(data.Value.Ship.ShipCategory)).ToDictionary(x => x.Key, x => x.Value);

        dictionary.AddRange(cachedWrappers.Where(x => !dictionary.ContainsKey(x.Key)));

        dictionary.AddRange(InitialiseShipBuildContainers(fullShipList.Where(data => !ContainsShipIndex(data.Index, filteredShips) &&
                                                                                     !ContainsShipIndex(data.Index, cachedWrappers) &&
                                                                                     SelectedTiers.Contains(data.Tier) &&
                                                                                     SelectedClasses.Contains(data.ShipClass) &&
                                                                                     SelectedNations.Contains(data.ShipNation) &&
                                                                                     SelectedCategories.Contains(data.ShipCategory))));

        wrappersCache.RemoveMany(cachedWrappers);
        FilteredShipList.RemoveMany(filteredShips);
        SelectedShipList.RemoveMany(FilteredShipList.Where(x => SelectedShipList.ContainsKey(x.Key) && !PinnedShipList.ContainsKey(x.Key)));
        wrappersCache.AddRange(FilteredShipList.Where(x => !wrappersCache.ContainsKey(x.Key)));

        SetSelectAndPinAllButtonsStatus(dictionary);

        FilteredShipList = dictionary;

        GetDataSectionsToDisplay();

        // Keep the return type as task in order to allow making the method async in the future if there is heavy filtering logic that needs to run asynchronously.
        return Task.CompletedTask;
    }

    public void ToggleShowPinnedShipOnly()
    {
        ShowPinnedShipsOnly = !ShowPinnedShipsOnly;
        if (ShowPinnedShipsOnly)
        {
            PinAllShips = true;
            SelectAllShips = FilteredShipList.Intersect(PinnedShipList).All(x => SelectedShipList.ContainsKey(x.Key));
        }
        else
        {
            SetSelectAndPinAllButtonsStatus();
        }
    }

    public async Task ToggleTierSelection(int value)
    {
        if (SelectedTiers.Contains(value))
        {
            SelectedTiers.Remove(value);
        }
        else
        {
            SelectedTiers.Add(value);
        }

        await ApplyFilters();
    }

    public async Task ToggleClassSelection(ShipClass value)
    {
        if (SelectedClasses.Contains(value))
        {
            SelectedClasses.Remove(value);
        }
        else
        {
            SelectedClasses.Add(value);
        }

        await ApplyFilters();
    }

    public async Task ToggleNationSelection(Nation value)
    {
        if (SelectedNations.Contains(value))
        {
            SelectedNations.Remove(value);
        }
        else
        {
            SelectedNations.Add(value);
        }

        await ApplyFilters();
    }

    public async Task ToggleCategorySelection(ShipCategory value)
    {
        if (SelectedCategories.Contains(value))
        {
            SelectedCategories.Remove(value);
        }
        else
        {
            SelectedCategories.Add(value);
        }

        await ApplyFilters();
    }

    public async Task ToggleAllTiers(bool activationState)
    {
        SelectedTiers.Clear();
        if (activationState)
        {
            SelectedTiers.AddRange(Enumerable.Range(1, 11));
        }

        await ApplyFilters();
    }

    public async Task ToggleAllClasses(bool activationState)
    {
        SelectedClasses.Clear();
        if (activationState)
        {
            SelectedClasses.AddRange(Enum.GetValues<ShipClass>().Except(new[] { ShipClass.Auxiliary }));
        }

        await ApplyFilters();
    }

    public async Task ToggleAllNations(bool activationState)
    {
        SelectedNations.Clear();
        if (activationState)
        {
            SelectedNations.AddRange(Enum.GetValues<Nation>().Except(new[] { Nation.Common }));
        }

        await ApplyFilters();
    }

    public async Task ToggleAllCategories(bool activationState)
    {
        SelectedCategories.Clear();
        if (activationState)
        {
            SelectedCategories.AddRange(Enum.GetValues<ShipCategory>().Except(new[] { ShipCategory.Disabled, ShipCategory.Clan }));
        }

        await ApplyFilters();
    }

    public Dictionary<Guid, GridDataWrapper> GetShipsToBeDisplayed()
    {
        return GetShipsToBeDisplayed(false);
    }

    public void EditBuilds(IEnumerable<KeyValuePair<Guid, GridDataWrapper>> newWrappers)
    {
        EditBuilds(newWrappers, false);
    }

    public Dictionary<Guid, GridDataWrapper> RemoveBuilds(IEnumerable<KeyValuePair<Guid, GridDataWrapper>> wrappers)
    {
        Dictionary<Guid, GridDataWrapper> warnings = new();
        Dictionary<Guid, GridDataWrapper> buildList = wrappers.ToDictionary(x => x.Key, x => x.Value);
        foreach (var wrapper in buildList)
        {
            if (FilteredShipList.Count(x => x.Value.Ship.Index.Equals(wrapper.Value.Ship.Index)) > 1)
            {
                FilteredShipList.Remove(wrapper.Key);

                PinnedShipList.Remove(wrapper.Key);

                if (wrappersCache.ContainsKey(wrapper.Key))
                {
                    wrappersCache[wrapper.Key] = wrapper.Value;
                }

                DispersionCache.Remove(wrapper.Key);
            }
            else
            {
                if (wrapper.Value.Build is not null)
                {
                    var err = ResetBuild(wrapper.Value);
                    EditBuilds(new Dictionary<Guid, GridDataWrapper> { { err.Id, err } });
                    warnings.Add(err.Id, err);
                }
                else
                {
                    warnings.Add(wrapper.Key, wrapper.Value);
                }
            }
        }

        SelectedShipList.Clear();
        SelectedShipList.AddRange(warnings);

        SetSelectAndPinAllButtonsStatus();

        return warnings;
    }

    public void ResetAllBuilds()
    {
        RemoveBuilds(FilteredShipList);
        SelectedShipList.Clear();
        Dictionary<Guid, GridDataWrapper> list = FilteredShipList.Where(x => x.Value.Build is not null).ToDictionary(x => x.Key, x => ResetBuild(x.Value));
        EditBuilds(list, true);
        SetSelectAndPinAllButtonsStatus();
    }

    public async Task AddPinnedShip(GridDataWrapper wrapper)
    {
        if (!PinnedShipList.ContainsKey(wrapper.Id))
        {
            PinnedShipList.Add(wrapper.Id, wrapper);
        }
        else
        {
            await RemovePinnedShip(wrapper);
        }

        SetSelectAndPinAllButtonsStatus();
    }

    public void AddSelectedShip(GridDataWrapper wrapper)
    {
        if (!SelectedShipList.ContainsKey(wrapper.Id))
        {
            SelectedShipList.Add(wrapper.Id, wrapper);
        }
        else
        {
            RemoveSelectedShip(wrapper);
        }

        SetSelectAndPinAllButtonsStatus();
    }

    public void ToggleUpgradedModules()
    {
        UseUpgradedModules = !UseUpgradedModules;
        ChangeModulesBatch();
    }

    public void ToggleHideShipsWithoutSelectedSection()
    {
        HideShipsWithoutSelectedSection = !HideShipsWithoutSelectedSection;
    }

    public async Task SelectDataSection(ShipComparisonDataSections dataSection)
    {
        SelectedDataSection = dataSection;
        await ApplyFilters();
    }

    public void SelectAllDisplayedShips()
    {
        SelectAllShips = !SelectAllShips;

        var list = GetShipsToBeDisplayed();

        if (!string.IsNullOrEmpty(searchString))
        {
            list = list.Where(x => appSettings.SelectedLanguage.CultureInfo.CompareInfo.IndexOf(localizer.GetGameLocalization(x.Value.Ship.Index + "_FULL").Localization, searchString, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1).ToDictionary(x => x.Key, x => x.Value);
        }

        if (SelectAllShips)
        {
            SelectedShipList.AddRange(list.Where(x => !SelectedShipList.ContainsKey(x.Key)));
        }
        else
        {
            SelectedShipList.RemoveMany(list.Where(x => SelectedShipList.ContainsKey(x.Key)));
        }
    }

    public async Task PinAllDisplayedShips()
    {
        PinAllShips = !PinAllShips;

        var list = FilteredShipList;

        if (!string.IsNullOrEmpty(searchString))
        {
            list = list.Where(x => appSettings.SelectedLanguage.CultureInfo.CompareInfo.IndexOf(localizer.GetGameLocalization(x.Value.Ship.Index + "_FULL").Localization, searchString, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1).ToDictionary(x => x.Key, x => x.Value);
        }

        if (PinAllShips)
        {
            PinnedShipList.AddRange(list.Where(x => !PinnedShipList.ContainsKey(x.Key)));
        }
        else
        {
            PinnedShipList.RemoveMany(list.Where(x => PinnedShipList.ContainsKey(x.Key)));
            await ApplyFilters();
        }
    }

    public void DuplicateSelectedShips()
    {
        foreach (var selectedShip in SelectedShipList)
        {
            var newWrapper = new GridDataWrapper(selectedShip.Value.ShipBuildContainer with { Id = Guid.NewGuid() });
            FilteredShipList.Add(newWrapper.Id, newWrapper);
            if (PinnedShipList.ContainsKey(selectedShip.Key))
            {
                PinnedShipList.Add(newWrapper.Id, newWrapper);
            }

            if (DispersionCache.ContainsKey(selectedShip.Key))
            {
                DispersionCache[newWrapper.Id] = DispersionCache[selectedShip.Key];
            }
        }

        SetSelectAndPinAllButtonsStatus();
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
            newWrapper = new(ShipBuildContainer.CreateNew(ship, null, null) with { ShipDataContainer = GetShipDataContainer(ship) });
        }
        else
        {
            return;
        }

        FilteredShipList.Add(newWrapper.Id, newWrapper);
        PinnedShipList.Add(newWrapper.Id, newWrapper);

        SetSelectAndPinAllButtonsStatus();
        GetDataSectionsToDisplay();
        SearchString = string.Empty;
        ResearchedShip = string.Empty;
        SearchedShips.Clear();
    }

    public void UpdateRange(double selectedValue)
    {
        Range = selectedValue;
    }

    private Dictionary<Guid, GridDataWrapper> GetShipsToBeDisplayed(bool disableHideShipsIfNoSelectedSection)
    {
        Dictionary<Guid, GridDataWrapper> list = ShowPinnedShipsOnly ? PinnedShipList : FilteredShipList;

        if (!disableHideShipsIfNoSelectedSection)
        {
            list = HideShipsIfNoSelectedSection(list);
        }

        CacheDispersion(list.Values);

        return list;
    }

    private void EditBuilds(IEnumerable<KeyValuePair<Guid, GridDataWrapper>> newWrappers, bool clearCache)
    {
        foreach (var wrapper in newWrappers)
        {
            FilteredShipList[wrapper.Key] = wrapper.Value;

            if (PinnedShipList.ContainsKey(wrapper.Key))
            {
                PinnedShipList[wrapper.Key] = wrapper.Value;
            }
            else if (ContainsShipIndex(wrapper.Value.Ship.Index, PinnedShipList))
            {
                PinnedShipList.Add(wrapper.Key, wrapper.Value);
            }

            if (!clearCache && wrappersCache.ContainsKey(wrapper.Key))
            {
                wrappersCache[wrapper.Key] = wrapper.Value;
            }

            if (SelectedShipList.ContainsKey(wrapper.Key))
            {
                SelectedShipList[wrapper.Key] = wrapper.Value;
            }
        }

        if (clearCache)
        {
            wrappersCache.Clear();
        }
    }

    private async Task RemovePinnedShip(GridDataWrapper wrapper)
    {
        PinnedShipList.Remove(wrapper.Id);
        await ApplyFilters();
    }

    private void RemoveSelectedShip(GridDataWrapper wrapper)
    {
        SelectedShipList.Remove(wrapper.Id);
    }

    private Dictionary<Guid, GridDataWrapper> InitialiseShipBuildContainers(IEnumerable<Ship> ships)
    {
        return ships.Select(ship => new GridDataWrapper(ShipBuildContainer.CreateNew(ship, null, null) with { ShipDataContainer = GetShipDataContainer(ship) })).ToDictionary(x => x.Id, x => x);
    }

    private void ChangeModulesBatch()
    {
        EditBuilds(FilteredShipList.Where(x => x.Value.Build is null).ToDictionary(x => x.Key, x => ResetBuild(x.Value)));
    }

    private List<ShipUpgrade> GetShipConfiguration(Ship ship)
    {
        List<ShipUpgrade> shipConfiguration = UseUpgradedModules
            ? ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
                .OrderBy(entry => entry.Key)
                .Select(entry => entry.Value)
                .Select(module => module.Last())
                .ToList()
            : ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
                .OrderBy(entry => entry.Key)
                .Select(entry => entry.Value)
                .Select(module => module.First())
                .ToList();
        return shipConfiguration;
    }

    private ShipDataContainer GetShipDataContainer(Ship ship)
    {
        return ShipDataContainer.CreateFromShip(ship, GetShipConfiguration(ship), new());
    }

    private Dictionary<Guid, GridDataWrapper> HideShipsIfNoSelectedSection(IEnumerable<KeyValuePair<Guid, GridDataWrapper>> list)
    {
        if (!hideShipsWithoutSelectedSection)
        {
            return list.ToDictionary(x => x.Key, x => x.Value);
        }

        Dictionary<Guid, GridDataWrapper> newList = SelectedDataSection switch
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

        newList.AddRange(PinnedShipList.Where(x => !newList.ContainsKey(x.Key)));
        return newList;
    }

    private void GetDataSectionsToDisplay()
    {
        List<ShipComparisonDataSections> dataSections = Enum.GetValues<ShipComparisonDataSections>().ToList();
        Dictionary<Guid, GridDataWrapper> shipList = GetShipsToBeDisplayed(true);

        if (!shipList.Any())
        {
            DataSections = new() { ShipComparisonDataSections.General };
        }
        else
        {
            foreach (var dataSection in Enum.GetValues<ShipComparisonDataSections>().Except(new[] { ShipComparisonDataSections.Maneuverability, ShipComparisonDataSections.Concealment, ShipComparisonDataSections.Survivability, ShipComparisonDataSections.General }))
            {
                switch (dataSection)
                {
                    case ShipComparisonDataSections.MainBattery:
                        if (!shipList.Any(x => x.Value.ShipDataContainer.MainBatteryDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.He:
                        if (!shipList.Any(x => x.Value.HeShell?.Damage is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.Ap:
                        if (!shipList.Any(x => x.Value.ApShell?.Damage is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.Sap:
                        if (!shipList.Any(x => x.Value.SapShell?.Damage is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.Torpedo:
                        if (!shipList.Any(x => x.Value.ShipDataContainer.TorpedoArmamentDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.SecondaryBattery:
                    case ShipComparisonDataSections.SecondaryBatteryShells:
                        if (!shipList.Any(x => x.Value.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.AntiAir:
                        if (!shipList.Any(x => x.Value.ShipDataContainer.AntiAirDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.Asw:
                        if (!shipList.Any(x => x.Value.ShipDataContainer.AswAirstrikeDataContainer is not null || x.Value.ShipDataContainer.DepthChargeLauncherDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.AirStrike:
                        if (!shipList.Any(x => x.Value.ShipDataContainer.AirstrikeDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.RocketPlanes:
                        if (!shipList.Any(x => x.Value.RocketPlanes.Type.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.Rockets:
                        if (!shipList.Any(x => x.Value.RocketPlanes.WeaponType.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.TorpedoBombers:
                        if (!shipList.Any(x => x.Value.TorpedoBombers.Type.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.AerialTorpedoes:
                        if (!shipList.Any(x => x.Value.TorpedoBombers.WeaponType.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.Bombers:
                        if (!shipList.Any(x => x.Value.Bombers.Type.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.Bombs:
                        if (!shipList.Any(x => x.Value.Bombers.WeaponType.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                    case ShipComparisonDataSections.Sonar:
                        if (!shipList.Any(x => x.Value.ShipDataContainer.PingerGunDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
                }
            }

            DataSections = dataSections;
        }
    }

    private void FindShips()
    {
        if (string.IsNullOrEmpty(researchedShip.Trim()))
        {
            return;
        }

        SearchedShips.Clear();
        SearchedShips.AddRange(fullShipList.Where(ship => appSettings.SelectedLanguage.CultureInfo.CompareInfo.IndexOf(localizer.GetGameLocalization(ship.Index + "_FULL").Localization, researchedShip, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1));
    }

    private void SetSelectAndPinAllButtonsStatus() => SetSelectAndPinAllButtonsStatus(GetShipsToBeDisplayed());

    private void SetSelectAndPinAllButtonsStatus(IReadOnlyDictionary<Guid, GridDataWrapper> list)
    {
        SelectAllShips = list.All(wrapper => SelectedShipList.ContainsKey(wrapper.Key));
        PinAllShips = list.All(wrapper => PinnedShipList.ContainsKey(wrapper.Key));
    }

    private GridDataWrapper ResetBuild(GridDataWrapper wrapper)
    {
        GridDataWrapper reset = new(wrapper.ShipBuildContainer with { Build = null, ActivatedConsumableSlots = null, SpecialAbilityActive = false, ShipDataContainer = GetShipDataContainer(wrapper.Ship), Modifiers = null });
        return reset;
    }

    // this is needed because dispersion is not calculated inside the GridDataWrapper in order to not recalculate oll of it upon each range variation
    private void CacheDispersion(IEnumerable<GridDataWrapper> wrappers)
    {
        foreach (var wrapper in wrappers)
        {
            if (wrapper.MainBattery?.DispersionData is not null && wrapper.MainBattery?.DispersionModifier is not null && wrapper.MainBattery?.Range is not null)
            {
                DispersionCache[wrapper.Id] = wrapper.MainBattery.DispersionData.CalculateDispersion(decimal.ToDouble(wrapper.MainBattery.Range * 1000), wrapper.MainBattery.DispersionModifier, Range * 1000);
            }
        }
    }
}
