using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.Helper;

namespace WoWsShipBuilder.ViewModels.Other;

public class ShipComparisonViewModel : ViewModelBase
{
    #region Properties

    private readonly IAppDataService appDataService;

    private readonly ILocalizer localizer;

    private List<Ship> fullShipList = new(AppData.ShipDictionary!.Values);

    public List<Ship> FullShipList
    {
        get => fullShipList;
        set => this.RaiseAndSetIfChanged(ref fullShipList, value);
    }

    private List<ShipComparisonDataWrapper> filteredShipList = new();

    public List<ShipComparisonDataWrapper> FilteredShipList
    {
        get => filteredShipList;
        set => this.RaiseAndSetIfChanged(ref filteredShipList, value);
    }

    private List<ShipComparisonDataWrapper> selectedShipList = new();

    public List<ShipComparisonDataWrapper> SelectedShipList
    {
        get => selectedShipList;
        set => this.RaiseAndSetIfChanged(ref selectedShipList, value);
    }

    private List<ShipComparisonDataWrapper> wrappersCache = new();

    public List<ShipComparisonDataWrapper> WrappersCache
    {
        get => wrappersCache;
        set => this.RaiseAndSetIfChanged(ref wrappersCache, value);
    }

    private List<ShipComparisonDataWrapper> pinnedShipList = new();

    public List<ShipComparisonDataWrapper> PinnedShipList
    {
        get => pinnedShipList;
        set => this.RaiseAndSetIfChanged(ref pinnedShipList, value);
    }

    private DataSections selectedDataSection = DataSections.General;

    public DataSections SelectedDataSection
    {
        get => selectedDataSection;
        set => this.RaiseAndSetIfChanged(ref selectedDataSection, value);
    }

    public ObservableCollection<int> SelectedTiers { get; } = new();

    public ObservableCollection<ShipClass> SelectedClasses { get; } = new();

    public ObservableCollection<Nation> SelectedNations { get; } = new();

    public ObservableCollection<ShipCategory> SelectedCategories { get; } = new();

    public IEnumerable<ShipClass> AvailableClasses { get; } = Enum.GetValues<ShipClass>().Except(new[] {ShipClass.Auxiliary});

    public IEnumerable<Nation> AvailableNations { get; } = Enum.GetValues<Nation>().Except(new[] {Nation.Common});

    public IEnumerable<ShipCategory> AvailableShipCategories { get; } = Enum.GetValues<ShipCategory>().Except(new[] {ShipCategory.Disabled, ShipCategory.Clan});

    public readonly string DefaultBuildName = "Default";

    private List<Ship> searchedShips = new();

    public List<Ship> SearchedShips
    {
        get => searchedShips;
        set => this.RaiseAndSetIfChanged(ref searchedShips, value);
    }

    private string searchShip = string.Empty;

    public string SearchShip
    {
        get => searchShip;
        set
        {
            searchShip = value;
            FindShips();
            this.RaiseAndSetIfChanged(ref searchShip, value);
        }
    }

    private string searchString = default!;

    public string SearchString
    {
        get => searchString;
        set => this.RaiseAndSetIfChanged(ref searchString, value);
    }

    private bool showPinnedShipsOnly;

    public bool ShowPinnedShipsOnly
    {
        get => showPinnedShipsOnly;
        set
        {
            GetShipsToBeDisplayedList();
            this.RaiseAndSetIfChanged(ref showPinnedShipsOnly, value);
        }
    }

    private bool pinAllShips;

    public bool PinAllShips
    {
        get => pinAllShips;
        set => this. RaiseAndSetIfChanged(ref pinAllShips, value);
    }

    private bool selectAllShips;

    public bool SelectAllShips
    {
        get => selectAllShips;
        set => this. RaiseAndSetIfChanged(ref selectAllShips, value);
    }

    private bool useUpgradedModules;

    public bool UseUpgradedModules
    {
        get => useUpgradedModules;
        set => this. RaiseAndSetIfChanged(ref useUpgradedModules, value);
    }

    #endregion

    public ShipComparisonViewModel(IAppDataService appDataService, ILocalizer localizer)
    {
        this.appDataService = appDataService;
        this.localizer = localizer;
    }

    private async Task<List<ShipComparisonDataWrapper>> ApplyFilters()
    {
        List<ShipComparisonDataWrapper> list = new();
        List<ShipComparisonDataWrapper> filteredShips = FilteredShipList.Where(data => SelectedTiers.Contains(data.Ship.Tier) &&
                                                                                       SelectedClasses.Contains(data.Ship.ShipClass) &&
                                                                                       SelectedNations.Contains(data.Ship.ShipNation) &&
                                                                                       SelectedCategories.Contains(data.Ship.ShipCategory)).ToList();
        list.AddRange(filteredShips);

        List<ShipComparisonDataWrapper> cachedWrappers = WrappersCache.Where(data => SelectedTiers.Contains(data.Ship.Tier) &&
                                                                       SelectedClasses.Contains(data.Ship.ShipClass) &&
                                                                       SelectedNations.Contains(data.Ship.ShipNation) &&
                                                                       SelectedCategories.Contains(data.Ship.ShipCategory)).ToList();
        list.AddRange(cachedWrappers);

        list.AddRange(await InitialiseShipComparisonDataWrapper(FullShipList
            .Where(data => !ContainsShipIndex(data.Index) &&
                           !ContainsShipIndex(data.Index, cachedWrappers) &&
                           SelectedTiers.Contains(data.Tier) &&
                           SelectedClasses.Contains(data.ShipClass) &&
                           SelectedNations.Contains(data.ShipNation) &&
                           SelectedCategories.Contains(data.ShipCategory))
            .ToList()));

        cachedWrappers.ForEach(x => WrappersCache.Remove(x));
        filteredShips.ForEach(x => FilteredShipList.Remove(x));
        FilteredShipList.Where(x => ContainsWrapper(x, SelectedShipList)).ToList().ForEach(x => SelectedShipList.Remove(x));
        FilteredShipList.Where(x => ContainsWrapper(x, PinnedShipList)).ToList().ForEach(x => PinnedShipList.Remove(x));
        WrappersCache.AddRange(FilteredShipList.Where(x => !ContainsWrapper(x, WrappersCache)));

        if (list.Count == 0)
        {
            SelectAllShips = false;
            PinAllShips = false;
        }

        return list;
    }

    public void ToggleShowPinnedShipOnly()
    {
        ShowPinnedShipsOnly = !ShowPinnedShipsOnly;
        if (ShowPinnedShipsOnly)
        {
            PinAllShips = true;
            SelectAllShips = FilteredShipList.Intersect(PinnedShipList).All(x => ContainsWrapper(x, SelectedShipList));
        }
        else
        {
            PinAllShips = FilteredShipList.All(x => ContainsWrapper(x, PinnedShipList));
            SelectAllShips = FilteredShipList.All(x => ContainsWrapper(x, SelectedShipList));
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

        FilteredShipList = await ApplyFilters();
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

        FilteredShipList = await ApplyFilters();
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

        FilteredShipList = await ApplyFilters();
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

        FilteredShipList = await ApplyFilters();
    }

    public async Task ToggleAllTiers(bool activationState)
    {
        SelectedTiers.Clear();
        if (activationState)
        {
            SelectedTiers.AddRange(Enumerable.Range(1, 11));
        }

        FilteredShipList = await ApplyFilters();
    }

    public async Task ToggleAllClasses(bool activationState)
    {
        SelectedClasses.Clear();
        if (activationState)
        {
            SelectedClasses.AddRange(Enum.GetValues<ShipClass>().Except(new[] { ShipClass.Auxiliary }));
        }

        FilteredShipList = await ApplyFilters();
    }

    public async Task ToggleAllNations(bool activationState)
    {
        SelectedNations.Clear();
        if (activationState)
        {
            SelectedNations.AddRange(Enum.GetValues<Nation>().Except(new[] { Nation.Common }));
        }

        FilteredShipList = await ApplyFilters();
    }

    public async Task ToggleAllCategories(bool activationState)
    {
        SelectedCategories.Clear();
        if (activationState)
        {
            SelectedCategories.AddRange(Enum.GetValues<ShipCategory>().Except(new[] { ShipCategory.Disabled, ShipCategory.Clan }));
        }

        FilteredShipList = await ApplyFilters();
    }

    public List<ShipComparisonDataWrapper> GetShipsToBeDisplayedList()
    {
        return ShowPinnedShipsOnly ? PinnedShipList : FilteredShipList;
    }

    public void EditBuilds(List<ShipComparisonDataWrapper> newWrappers)
    {
        foreach (var wrapper in newWrappers)
        {
            if (ContainsWrapper(wrapper))
            {
                FilteredShipList.Replace(FilteredShipList.First(x => x.Id.Equals(wrapper.Id)), wrapper);
            }
            else
            {
                FilteredShipList.Add(wrapper);
            }

            if (ContainsWrapper(wrapper, PinnedShipList))
            {
                PinnedShipList.Replace(PinnedShipList.First(x => x.Id.Equals(wrapper.Id)), wrapper);
            }
            else if (ContainsShipIndex(wrapper.Ship.Index, PinnedShipList))
            {
                PinnedShipList.Add(wrapper);
            }

            if (ContainsWrapper(wrapper, WrappersCache))
            {
                WrappersCache.Replace(WrappersCache.First(x => x.Id.Equals(wrapper.Id)), wrapper);
            }

            if (ContainsWrapper(wrapper, SelectedShipList))
            {
                SelectedShipList.Replace(SelectedShipList.First(x => x.Id.Equals(wrapper.Id)), wrapper);
            }
        }
    }

    public async Task<List<string>> RemoveBuilds()
    {
        List<string> warning = new();
        List<ShipComparisonDataWrapper> list = new(SelectedShipList);
        foreach (var wrapper in list)
        {
            if (FilteredShipList.FindAll(x => x.Ship.Index.Equals(wrapper.Ship.Index)).Count > 1)
            {
                FilteredShipList.Remove(wrapper);
                if (ContainsWrapper(wrapper, PinnedShipList))
                {
                    PinnedShipList.Remove(wrapper);
                }
                if (ContainsWrapper(wrapper, WrappersCache))
                {
                    WrappersCache.Replace(SelectWrapper(wrapper, WrappersCache), wrapper);
                }
            }
            else
            {
                if (wrapper.Build is not null)
                {
                    EditBuilds(new(){new(wrapper.Ship, await GetShipConfiguration(wrapper.Ship), null, wrapper.Id)});
                }
                warning.Add(wrapper.Ship.Index);
            }
        }
        SelectedShipList.Clear();
        selectAllShips = false;

        return warning;
    }

    public void AddPinnedShip(ShipComparisonDataWrapper wrapper)
    {
        List<ShipComparisonDataWrapper> list = GetShipsToBeDisplayedList();
        if (!ContainsWrapper(wrapper, PinnedShipList))
        {
            PinnedShipList.Add(wrapper);
            PinAllShips = list.Count == PinnedShipList.Count;
        }
        else
        {
            RemovePinnedShip(wrapper);
            PinAllShips = ShowPinnedShipsOnly || PinnedShipList.Count == 0 && false;
            if (ShowPinnedShipsOnly)
            {
                list = FilteredShipList.Intersect(PinnedShipList).ToList();
            }
            SelectAllShips = list.All(x => ContainsWrapper(x, SelectedShipList));
        }
    }

    public void RemovePinnedShip(ShipComparisonDataWrapper wrapper)
    {
        PinnedShipList.Remove(wrapper);
    }

    public void AddSelectedShip(ShipComparisonDataWrapper wrapper)
    {
        List<ShipComparisonDataWrapper> list = GetShipsToBeDisplayedList();

        if (!ContainsWrapper(wrapper, SelectedShipList))
        {
            SelectedShipList.Add(wrapper);
            if (ShowPinnedShipsOnly)
            {
                list = FilteredShipList.Intersect(PinnedShipList).ToList();
            }
            SelectAllShips = list.All(x => ContainsWrapper(x, SelectedShipList));
        }
        else
        {
            RemoveSelectedShip(wrapper);
            SelectAllShips = SelectedShipList.Count == 0 && false;
        }
    }

    public void RemoveSelectedShip(ShipComparisonDataWrapper wrapper)
    {
        SelectedShipList.Remove(wrapper);
    }

    public bool ContainsWrapper(ShipComparisonDataWrapper wrapper, IEnumerable<ShipComparisonDataWrapper>? list = null)
    {
        list ??= FilteredShipList;
        return list.Select(x => x.Id).Contains(wrapper.Id);
    }

    public bool ContainsShipIndex(string shipIndex, IEnumerable<ShipComparisonDataWrapper>? list = null)
    {
        list ??= FilteredShipList;
        return list.Select(x => x.Ship.Index).Contains(shipIndex);
    }

    public ShipComparisonDataWrapper SelectWrapper(ShipComparisonDataWrapper wrapper, IEnumerable<ShipComparisonDataWrapper>? list = null)
    {
        list ??= FilteredShipList;
        return list.First(x => x.Id.Equals(wrapper.Id));
    }

    public async Task<List<ShipComparisonDataWrapper>> InitialiseShipComparisonDataWrapper(List<Ship> ships)
    {
        List<ShipComparisonDataWrapper> list = new();
        foreach (var ship in ships)
        {
            list.Add(new(ship, await GetShipConfiguration(ship)));
        }

        return list;
    }

    public async Task<List<ShipComparisonDataWrapper>> ChangeModulesBatch(List<ShipComparisonDataWrapper> wrappers)
    {
        List<ShipComparisonDataWrapper> list = new();
        foreach (var wrapper in wrappers)
        {
            list.Add(wrapper with {ShipDataContainer = await GetShipConfiguration(wrapper.Ship)});
        }

        return list;
    }

    public async Task<ShipDataContainer> GetShipConfiguration(Ship ship)
    {
        List<ShipUpgrade> shipConfiguration = UseUpgradedModules ?
            ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
                .OrderBy(entry => entry.Key)
                .Select(entry => entry.Value)
                .Select(module => module.Last())
                .ToList() :
            ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
                .OrderBy(entry => entry.Key)
                .Select(entry => entry.Value)
                .Select(module => module.First())
                .ToList();
        return await ShipDataContainer.FromShipAsync(ship, shipConfiguration, new(), appDataService);
    }

    public async Task ToggleUpgradedModules()
    {
        UseUpgradedModules = !UseUpgradedModules;
        EditBuilds(await ChangeModulesBatch(FilteredShipList));
    }

    public enum DataSections
    {
        General,
        MainBattery,
        He,
        Sap,
        Ap,
        RocketPlanes,
        TorpedoBombers,
        DiveBombers,
        PingerGun,
        Torpedo,
        SecondaryBattery,
        AntiAir,
        AirStrike,
        AswStrike,
        DepthCharge,
        Maneuverability,
        Concealment,
        Survivability,
    }

    public void ToggleDataSection(DataSections dataSection)
    {
        selectedDataSection = dataSection;
    }

    public void SelectAllDisplayedShips()
    {
        SelectAllShips = !SelectAllShips;

        List<ShipComparisonDataWrapper> list = GetShipsToBeDisplayedList();

        if (!string.IsNullOrEmpty(searchString))
        {
            list = list.Where(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(localizer.GetGameLocalization(x.Ship.Index).Localization, searchString, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1).ToList();
        }

        if (SelectAllShips)
        {
            SelectedShipList.AddRange(list.Where(x => !ContainsWrapper(x, SelectedShipList)));
        }
        else
        {
            list.Where(x => ContainsWrapper(x, SelectedShipList)).ToList().ForEach(x => SelectedShipList.Remove(x));
        }
    }

    public void PinAllDisplayedShips()
    {
        PinAllShips = !PinAllShips;

        List<ShipComparisonDataWrapper> list = FilteredShipList;

        if (!string.IsNullOrEmpty(searchString))
        {
            list = list.Where(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(localizer.GetGameLocalization(x.Ship.Index).Localization, searchString, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1).ToList();
        }

        if (PinAllShips)
        {
            PinnedShipList.AddRange(list.Where(x => !ContainsWrapper(x, PinnedShipList)));
        }
        else
        {
            list.Where(x => ContainsWrapper(x, PinnedShipList)).ToList().ForEach(x => PinnedShipList.Remove(x));
        }
    }

    private void FindShips()
    {
        if (string.IsNullOrEmpty(searchShip.Trim()))
        {
            return;
        }
        searchedShips.Clear();
        searchedShips.AddRange(FullShipList.Where(ship => CultureInfo.CurrentCulture.CompareInfo.IndexOf(localizer.GetGameLocalization(ship.Index).Localization, searchShip, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1));
    }

    public void DuplicateSelectedShips()
    {
        List<ShipComparisonDataWrapper> list = SelectedShipList;
        foreach (var selectedShip in list)
        {
            ShipComparisonDataWrapper newWrapper = new(selectedShip.Ship, selectedShip.ShipDataContainer, selectedShip.Build);
            FilteredShipList.Add(newWrapper);
            if (ContainsWrapper(selectedShip, PinnedShipList))
            {
                PinnedShipList.Add(newWrapper);
            }
        }
    }

    public async Task AddShip(object? obj)
    {
        if (obj is not Ship ship) return;

        var reApplyFilters = false;

        if (!SelectedTiers.Contains(ship.Tier))
        {
            SelectedTiers.Add(ship.Tier);
            reApplyFilters = true;
        }

        if (!SelectedNations.Contains(ship.ShipNation))
        {
            SelectedNations.Add(ship.ShipNation);
            reApplyFilters = true;
        }

        if (!SelectedClasses.Contains(ship.ShipClass))
        {
            SelectedClasses.Add(ship.ShipClass);
            reApplyFilters = true;
        }

        if (!SelectedCategories.Contains(ship.ShipCategory))
        {
            SelectedCategories.Add(ship.ShipCategory);
            reApplyFilters = true;
        }

        if (reApplyFilters)
        {
            FilteredShipList = await ApplyFilters();
        }

        PinnedShipList.Where(x => x.Ship.Index.Equals(ship.Index)).ToList().ForEach(x => PinnedShipList.Remove(x));
        PinnedShipList.AddRange(FilteredShipList.Where(x => x.Ship.Index.Equals(ship.Index)));

        SearchString = string.Empty;
        ShowPinnedShipsOnly = true;
        PinAllShips = true;
        SelectAllShips = FilteredShipList.Intersect(PinnedShipList).All(x => ContainsWrapper(x, SelectedShipList));
        SearchShip = string.Empty;
        SearchedShips.Clear();
    }
}
