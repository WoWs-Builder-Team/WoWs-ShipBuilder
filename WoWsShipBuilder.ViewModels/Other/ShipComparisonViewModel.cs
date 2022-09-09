using System;
using System.Collections.ObjectModel;
using System.Globalization;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.Helper;

namespace WoWsShipBuilder.ViewModels.Other;

public class ShipComparisonViewModel : ViewModelBase
{
    public const string DataNotAvailable = "N/A";

    public static readonly string DefaultBuildName = "---";

    private readonly IAppDataService appDataService;

    private readonly ILocalizer localizer;

    private readonly AppSettings appSettings;

    private readonly IEnumerable<Ship> fullShipList = AppData.ShipDictionary!.Values;

    private List<ShipComparisonDataWrapper> filteredShipList = new();

    public List<ShipComparisonDataWrapper> FilteredShipList
    {
        get => filteredShipList;
        private set => this.RaiseAndSetIfChanged(ref filteredShipList, value);
    }

    public List<ShipComparisonDataWrapper> SelectedShipList { get; } = new();

    public List<ShipComparisonDataWrapper> PinnedShipList { get; } = new();

    public DataSections SelectedDataSection { get; private set; } = DataSections.General;

    public ObservableCollection<int> SelectedTiers { get; } = new();

    public ObservableCollection<ShipClass> SelectedClasses { get; } = new();

    public ObservableCollection<Nation> SelectedNations { get; } = new();

    public ObservableCollection<ShipCategory> SelectedCategories { get; } = new();

    public IEnumerable<ShipClass> AvailableClasses { get; } = Enum.GetValues<ShipClass>().Except(new[] {ShipClass.Auxiliary});

    public IEnumerable<Nation> AvailableNations { get; } = Enum.GetValues<Nation>().Except(new[] {Nation.Common});

    public IEnumerable<ShipCategory> AvailableShipCategories { get; } = Enum.GetValues<ShipCategory>().Except(new[] {ShipCategory.Disabled, ShipCategory.Clan});

    private readonly List<ShipComparisonDataWrapper> wrappersCache = new();

    public List<Ship> SearchedShips { get; } = new();

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
        private set
        {
            GetShipsToBeDisplayedList();
            this.RaiseAndSetIfChanged(ref showPinnedShipsOnly, value);
        }
    }

    private bool pinAllShips;

    public bool PinAllShips
    {
        get => pinAllShips;
        private set => this. RaiseAndSetIfChanged(ref pinAllShips, value);
    }

    private bool selectAllShips;

    public bool SelectAllShips
    {
        get => selectAllShips;
        private set => this. RaiseAndSetIfChanged(ref selectAllShips, value);
    }

    private bool useUpgradedModules;

    public bool UseUpgradedModules
    {
        get => useUpgradedModules;
        private set => this. RaiseAndSetIfChanged(ref useUpgradedModules, value);
    }

    public ShipComparisonViewModel(IAppDataService appDataService, ILocalizer localizer, AppSettings appSettings)
    {
        this.appDataService = appDataService;
        this.localizer = localizer;
        this.appSettings = appSettings;
    }

    private async Task<List<ShipComparisonDataWrapper>> ApplyFilters()
    {
        List<ShipComparisonDataWrapper> list = new();

        list.AddRange(PinnedShipList);

        List<ShipComparisonDataWrapper> filteredShips = FilteredShipList.Where(data => SelectedTiers.Contains(data.Ship.Tier) &&
                                                                                       SelectedClasses.Contains(data.Ship.ShipClass) &&
                                                                                       SelectedNations.Contains(data.Ship.ShipNation) &&
                                                                                       SelectedCategories.Contains(data.Ship.ShipCategory)).ToList();

        list.AddRange(filteredShips.Where(x => !ContainsWrapper(x, list)));

        List<ShipComparisonDataWrapper> cachedWrappers = wrappersCache.Where(data => SelectedTiers.Contains(data.Ship.Tier) &&
                                                                       SelectedClasses.Contains(data.Ship.ShipClass) &&
                                                                       SelectedNations.Contains(data.Ship.ShipNation) &&
                                                                       SelectedCategories.Contains(data.Ship.ShipCategory)).ToList();

        list.AddRange(cachedWrappers.Where(x => !ContainsWrapper(x, list)));

        list.AddRange(await InitialiseShipComparisonDataWrapper(fullShipList
            .Where(data => !ContainsShipIndex(data.Index) &&
                           !ContainsShipIndex(data.Index, cachedWrappers) &&
                           SelectedTiers.Contains(data.Tier) &&
                           SelectedClasses.Contains(data.ShipClass) &&
                           SelectedNations.Contains(data.ShipNation) &&
                           SelectedCategories.Contains(data.ShipCategory))
            .ToList()));

        cachedWrappers.ForEach(x => wrappersCache.Remove(x));
        filteredShips.ForEach(x => FilteredShipList.Remove(x));
        FilteredShipList.Where(x => ContainsWrapper(x, SelectedShipList) && !ContainsWrapper(x, PinnedShipList)).ToList().ForEach(x => SelectedShipList.Remove(x));
        wrappersCache.AddRange(FilteredShipList.Where(x => !ContainsWrapper(x, wrappersCache)));

        SetSelectAndPinAllButtonsStatus(list);

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

    public void EditBuilds(List<ShipComparisonDataWrapper> newWrappers, bool clearCache = false)
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

            if (!clearCache && ContainsWrapper(wrapper, wrappersCache))
            {
                wrappersCache.Replace(wrappersCache.First(x => x.Id.Equals(wrapper.Id)), wrapper);
            }

            if (ContainsWrapper(wrapper, SelectedShipList))
            {
                SelectedShipList.Replace(SelectedShipList.First(x => x.Id.Equals(wrapper.Id)), wrapper);
            }
        }
        if (clearCache)
        {
            wrappersCache.Clear();
        }
    }

    public async Task<List<ShipComparisonDataWrapper>> RemoveBuilds(IEnumerable<ShipComparisonDataWrapper> builds)
    {
        List<ShipComparisonDataWrapper> warning = new();
        List<ShipComparisonDataWrapper> list = new(builds);
        foreach (var wrapper in list)
        {
            if (FilteredShipList.FindAll(x => x.Ship.Index.Equals(wrapper.Ship.Index)).Count > 1)
            {
                FilteredShipList.Remove(wrapper);
                if (ContainsWrapper(wrapper, PinnedShipList))
                {
                    PinnedShipList.Remove(wrapper);
                }
                if (ContainsWrapper(wrapper, wrappersCache))
                {
                    wrappersCache.Replace(SelectWrapper(wrapper, wrappersCache), wrapper);
                }
            }
            else
            {
                if (wrapper.Build is not null)
                {
                    ShipComparisonDataWrapper err = new(wrapper.Ship, await GetShipConfiguration(wrapper.Ship), null, wrapper.Id);
                    EditBuilds(new(){err});
                    warning.Add(err);
                }
                else
                {
                    warning.Add(wrapper);
                }
            }
        }
        SelectedShipList.Clear();
        SelectedShipList.AddRange(warning);

        SetSelectAndPinAllButtonsStatus();

        return warning;
    }

    public async Task ResetAllBuilds()
    {
        await RemoveBuilds(FilteredShipList);
        SelectedShipList.Clear();
        ShipComparisonDataWrapper[] list = await Task.WhenAll(FilteredShipList.Where(x => x.Build is not null).Select(async x => new ShipComparisonDataWrapper(x.Ship, await GetShipConfiguration(x.Ship), null, x.Id)));
        EditBuilds(list.ToList(), true);
    }

    public async Task AddPinnedShip(ShipComparisonDataWrapper wrapper)
    {
        List<ShipComparisonDataWrapper> list = GetShipsToBeDisplayedList();
        if (!ContainsWrapper(wrapper, PinnedShipList))
        {
            PinnedShipList.Add(wrapper);
            PinAllShips = list.Count == PinnedShipList.Count;
        }
        else
        {
            await RemovePinnedShip(wrapper);
            PinAllShips = ShowPinnedShipsOnly || PinnedShipList.Count == 0 && false;
            if (ShowPinnedShipsOnly)
            {
                list = FilteredShipList.Intersect(PinnedShipList).ToList();
            }
            SelectAllShips = list.All(x => ContainsWrapper(x, SelectedShipList));
        }
    }

    private async Task RemovePinnedShip(ShipComparisonDataWrapper wrapper)
    {
        PinnedShipList.Remove(wrapper);
        FilteredShipList = await ApplyFilters();
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

    private void RemoveSelectedShip(ShipComparisonDataWrapper wrapper)
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

    private async Task<List<ShipComparisonDataWrapper>> InitialiseShipComparisonDataWrapper(List<Ship> ships)
    {
        List<ShipComparisonDataWrapper> list = new();
        foreach (var ship in ships)
        {
            list.Add(new(ship, await GetShipConfiguration(ship)));
        }

        return list;
    }

    private async Task ChangeModulesBatch()
    {
        ShipComparisonDataWrapper[] list = await Task.WhenAll(FilteredShipList.Where(x => x.Build is null).Select(async x => new ShipComparisonDataWrapper(x.Ship, await GetShipConfiguration(x.Ship), null, x.Id)));
        EditBuilds(list.ToList());
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
        await ChangeModulesBatch();
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
        Consumables,
    }

    public void ToggleDataSection(DataSections dataSection)
    {
        SelectedDataSection = dataSection;
    }

    public void SelectAllDisplayedShips()
    {
        SelectAllShips = !SelectAllShips;

        List<ShipComparisonDataWrapper> list = GetShipsToBeDisplayedList();

        if (!string.IsNullOrEmpty(searchString))
        {
            list = list.Where(x => appSettings.SelectedLanguage.CultureInfo.CompareInfo.IndexOf(localizer.GetGameLocalization(x.ShipIndex + "_FULL").Localization, searchString, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1).ToList();
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
            list = list.Where(x => appSettings.SelectedLanguage.CultureInfo.CompareInfo.IndexOf(localizer.GetGameLocalization(x.ShipIndex + "_FULL").Localization, searchString, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1).ToList();
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
        SearchedShips.Clear();
        SearchedShips.AddRange(fullShipList.Where(ship => appSettings.SelectedLanguage.CultureInfo.CompareInfo.IndexOf(localizer.GetGameLocalization(ship.Index + "_FULL").Localization, searchShip, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1));
    }

    public void DuplicateSelectedShips()
    {
        foreach (var selectedShip in SelectedShipList)
        {
            ShipComparisonDataWrapper newWrapper = new(selectedShip.Ship, selectedShip.ShipDataContainer, selectedShip.Build);
            FilteredShipList.Add(newWrapper);
            if (ContainsWrapper(selectedShip, PinnedShipList))
            {
                PinnedShipList.Add(newWrapper);
            }
        }
        SetSelectAndPinAllButtonsStatus();
    }

    public async Task AddShip(object? obj)
    {
        if (obj is not Ship ship) return;

        ShipComparisonDataWrapper newWrapper = new(ship, await GetShipConfiguration(ship));
        FilteredShipList.Add(newWrapper);
        PinnedShipList.Add(newWrapper);

        SetSelectAndPinAllButtonsStatus();
        SearchString = string.Empty;
        SearchShip = string.Empty;
        SearchedShips.Clear();
    }

    private void SetSelectAndPinAllButtonsStatus(IReadOnlyCollection<ShipComparisonDataWrapper>? list = null)
    {
        list ??= GetShipsToBeDisplayedList();
        SelectAllShips = list.Where(wrapper => !ContainsWrapper(wrapper, SelectedShipList)).ToList().Count == 0;
        PinAllShips = list.Where(wrapper => !ContainsWrapper(wrapper, PinnedShipList)).ToList().Count == 0;
    }
}
