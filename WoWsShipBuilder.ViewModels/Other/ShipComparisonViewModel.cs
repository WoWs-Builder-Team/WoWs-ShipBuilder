using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.Other;

public partial class ShipComparisonViewModel : ViewModelBase
{
    public const string DataNotAvailable = "N/A";

    public const string DefaultBuildName = "---";

    private readonly ILocalizer localizer;

    private readonly AppSettings appSettings;

    private readonly IEnumerable<Ship> fullShipList = AppData.ShipDictionary.Values;

    private Dictionary<Guid, ShipBuildContainer> filteredShipList = new();

    private Dictionary<Guid, ShipBuildContainer> FilteredShipList
    {
        get => filteredShipList;
        set => this.RaiseAndSetIfChanged(ref filteredShipList, value);
    }

    public Dictionary<Guid, ShipBuildContainer> SelectedShipList { get; } = new();

    public Dictionary<Guid, ShipBuildContainer> PinnedShipList { get; } = new();

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

    private readonly Dictionary<Guid, ShipBuildContainer> wrappersCache = new();

    private string researchedShip = string.Empty;

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

    [Observable]
    private string searchString = default!;

    [Observable]
    private bool showPinnedShipsOnly;

    [Observable]
    private bool pinAllShips;

    [Observable]
    private bool selectAllShips;

    [Observable]
    private bool useUpgradedModules;

    [Observable]
    private bool hideShipsWithoutSelectedSection;

    public ShipComparisonViewModel(ILocalizer localizer, AppSettings appSettings)
    {
        this.localizer = localizer;
        this.appSettings = appSettings;
    }

    public Task ApplyFilters()
    {
        Dictionary<Guid, ShipBuildContainer> dictionary = new();

        dictionary.AddRange(PinnedShipList);

        var filteredShips = FilteredShipList.Where(data => SelectedTiers.Contains(data.Value.Ship.Tier) &&
                                                                                            SelectedClasses.Contains(data.Value.Ship.ShipClass) &&
                                                                                            SelectedNations.Contains(data.Value.Ship.ShipNation) &&
                                                                                            SelectedCategories.Contains(data.Value.Ship.ShipCategory)).ToDictionary(x => x.Key, x => x.Value);

        dictionary.AddRange(filteredShips.Where(x => !dictionary.ContainsKey(x.Key)));

        Dictionary<Guid, ShipBuildContainer> cachedWrappers = wrappersCache.Where(data => SelectedTiers.Contains(data.Value.Ship.Tier) &&
                                                                                          SelectedClasses.Contains(data.Value.Ship.ShipClass) &&
                                                                                          SelectedNations.Contains(data.Value.Ship.ShipNation) &&
                                                                                          SelectedCategories.Contains(data.Value.Ship.ShipCategory)).ToDictionary(x => x.Key, x => x.Value);

        dictionary.AddRange(cachedWrappers.Where(x => !dictionary.ContainsKey(x.Key)));

        dictionary.AddRange(InitialiseShipComparisonDataWrapper(fullShipList.Where(data => !ContainsShipIndex(data.Index, filteredShips) &&
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

    public Dictionary<Guid, ShipBuildContainer> GetShipsToBeDisplayed()
    {
        return GetShipsToBeDisplayed(false);
    }

    private Dictionary<Guid, ShipBuildContainer> GetShipsToBeDisplayed(bool disableHideShipsIfNoSelectedSection)
    {
        Dictionary<Guid, ShipBuildContainer> list = ShowPinnedShipsOnly ? PinnedShipList : FilteredShipList;

        if (!disableHideShipsIfNoSelectedSection)
        {
            list = HideShipsIfNoSelectedSection(list);
        }

        return list;
    }

    public void EditBuilds(IEnumerable<KeyValuePair<Guid, ShipBuildContainer>> newWrappers)
    {
        EditBuilds(newWrappers, false);
    }

    private void EditBuilds(IEnumerable<KeyValuePair<Guid, ShipBuildContainer>> newWrappers, bool clearCache)
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

    public Dictionary<Guid, ShipBuildContainer> RemoveBuilds(IEnumerable<KeyValuePair<Guid, ShipBuildContainer>> wrappers)
    {
        Dictionary<Guid, ShipBuildContainer> warnings = new();
        Dictionary<Guid, ShipBuildContainer> buildList = wrappers.ToDictionary(x => x.Key, x=> x.Value);
        foreach (var wrapper in buildList)
        {
            if (FilteredShipList.Count(x => x.Value.Ship.Index.Equals(wrapper.Value.Ship.Index)) > 1)
            {
                FilteredShipList.Remove(wrapper.Key);

                if (PinnedShipList.ContainsKey(wrapper.Key))
                {
                    PinnedShipList.Remove(wrapper.Key);
                }

                if (wrappersCache.ContainsKey(wrapper.Key))
                {
                    wrappersCache[wrapper.Key] = wrapper.Value;
                }
            }
            else
            {
                if (wrapper.Value.Build is not null)
                {
                    var err = ResetBuild(wrapper.Value);
                    EditBuilds(new Dictionary<Guid, ShipBuildContainer> { { err.Id, err } });
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
        Dictionary<Guid, ShipBuildContainer> list = FilteredShipList.Where(x => x.Value.Build is not null).ToDictionary(x => x.Key, x => ResetBuild(x.Value));
        EditBuilds(list, true);
        SetSelectAndPinAllButtonsStatus();
    }

    public async Task AddPinnedShip(ShipBuildContainer wrapper)
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

    private async Task RemovePinnedShip(ShipBuildContainer wrapper)
    {
        PinnedShipList.Remove(wrapper.Id);
        await ApplyFilters();
    }

    public void AddSelectedShip(ShipBuildContainer wrapper)
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

    private void RemoveSelectedShip(ShipBuildContainer wrapper)
    {
        SelectedShipList.Remove(wrapper.Id);
    }

    private static bool ContainsShipIndex(string shipIndex, IEnumerable<KeyValuePair<Guid, ShipBuildContainer>> list)
    {
        return list.Select(x => x.Value.Ship.Index).Contains(shipIndex);
    }

    private Dictionary<Guid, ShipBuildContainer> InitialiseShipComparisonDataWrapper(IEnumerable<Ship> ships)
    {
        return ships.Select(ship => ShipBuildContainer.CreateNew(ship, null, null) with { ShipDataContainer = GetShipDataContainer(ship) }).ToDictionary(x => x.Id, x => x);
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

    public void ToggleUpgradedModules()
    {
        UseUpgradedModules = !UseUpgradedModules;
        ChangeModulesBatch();
    }

    public void ToggleHideShipsWithoutSelectedSection()
    {
        HideShipsWithoutSelectedSection = !HideShipsWithoutSelectedSection;
    }

    private bool HasDataSection(ShipBuildContainer shipBuildContainer) => HasDataSection(shipBuildContainer, SelectedDataSection);

    private static bool HasDataSection(ShipBuildContainer shipBuildContainer, ShipComparisonDataSections dataSection)
    {
        var shipDataContainer = shipBuildContainer.ShipDataContainer ?? throw new InvalidDataException("ShipDataContainer is null. ShipDataContainer must not be null.");
        var mainBattery = shipDataContainer.MainBatteryDataContainer;
        var heShellData = mainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.HE.ShellTypeToString()}"));
        var apShellData = mainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.AP.ShellTypeToString()}"));
        var sapShellData = mainBattery?.ShellData.FirstOrDefault(x => x.Type.Equals($"ArmamentType_{ShellType.SAP.ShellTypeToString()}"));
        var torpedoArmament = shipBuildContainer.ShipDataContainer.TorpedoArmamentDataContainer;
        var secondaryBattery = shipBuildContainer.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries;
        var aaData = shipBuildContainer.ShipDataContainer.AntiAirDataContainer;
        var airStrike = shipBuildContainer.ShipDataContainer.AirstrikeDataContainer;
        var aswAirStrike = shipBuildContainer.ShipDataContainer.AswAirstrikeDataContainer;
        var depthChargeLauncher = shipBuildContainer.ShipDataContainer.DepthChargeLauncherDataContainer;
        var sonar = shipBuildContainer.ShipDataContainer.PingerGunDataContainer;
        var rocketPlanes = shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Rocket.ProjectileTypeToString()));
        var torpedoBombers = shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Torpedo.ProjectileTypeToString()));
        var bombers = shipBuildContainer.ShipDataContainer.CvAircraftDataContainer?.Where(x => x.WeaponType.Equals(ProjectileType.Bomb.ProjectileTypeToString()) || x.WeaponType.Equals(ProjectileType.SkipBomb.ProjectileTypeToString()));

        return dataSection switch
        {
            ShipComparisonDataSections.MainBattery => mainBattery is not null,
            ShipComparisonDataSections.He => heShellData is not null,
            ShipComparisonDataSections.Ap => apShellData is not null,
            ShipComparisonDataSections.Sap => sapShellData is not null,
            ShipComparisonDataSections.Torpedo => torpedoArmament is not null,
            ShipComparisonDataSections.RocketPlanes => rocketPlanes is not null && rocketPlanes.Any(),
            ShipComparisonDataSections.Rockets => rocketPlanes is not null && rocketPlanes.Any(),
            ShipComparisonDataSections.TorpedoBombers => torpedoBombers is not null && torpedoBombers.Any(),
            ShipComparisonDataSections.AerialTorpedoes => torpedoBombers is not null && torpedoBombers.Any(),
            ShipComparisonDataSections.Bombers => bombers is not null && bombers.Any(),
            ShipComparisonDataSections.Bombs => bombers is not null && bombers.Any(),
            ShipComparisonDataSections.Sonar => sonar is not null,
            ShipComparisonDataSections.SecondaryBattery => secondaryBattery is not null,
            ShipComparisonDataSections.SecondaryBatteryShells => secondaryBattery is not null,
            ShipComparisonDataSections.AntiAir => aaData is not null,
            ShipComparisonDataSections.AirStrike => airStrike is not null,
            ShipComparisonDataSections.Asw => aswAirStrike is not null || depthChargeLauncher is not null,
            _ => true,
        };
    }

    private Dictionary<Guid, ShipBuildContainer> HideShipsIfNoSelectedSection(IEnumerable<KeyValuePair<Guid, ShipBuildContainer>> list)
    {
        if (!HideShipsWithoutSelectedSection)
        {
            return list.ToDictionary(x => x.Key, x => x.Value);
        }

        Dictionary<Guid, ShipBuildContainer> newList = list.Where(x => HasDataSection(x.Value)).ToDictionary(x => x.Key, x => x.Value);
        newList.AddRange(PinnedShipList.Where(x => newList.ContainsKey(x.Key)));
        return newList;
    }

    private void GetDataSectionsToDisplay()
    {
        List<ShipComparisonDataSections> dataSections = Enum.GetValues<ShipComparisonDataSections>().ToList();
        Dictionary<Guid, ShipBuildContainer> shipList = GetShipsToBeDisplayed(true);

        if (!shipList.Any())
        {
            DataSections = new() { ShipComparisonDataSections.General };
        }
        else
        {
            foreach (var dataSection in Enum.GetValues<ShipComparisonDataSections>().Except(new[] { ShipComparisonDataSections.Maneuverability, ShipComparisonDataSections.Concealment, ShipComparisonDataSections.Survivability, ShipComparisonDataSections.General }))
            {
                if (!shipList.Any(item => HasDataSection(item.Value, dataSection)))
                {
                    dataSections.Remove(dataSection);
                }
            }

            DataSections = dataSections;
        }
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

    private void FindShips()
    {
        if (string.IsNullOrEmpty(researchedShip.Trim()))
        {
            return;
        }

        SearchedShips.Clear();
        SearchedShips.AddRange(fullShipList.Where(ship => appSettings.SelectedLanguage.CultureInfo.CompareInfo.IndexOf(localizer.GetGameLocalization(ship.Index + "_FULL").Localization, researchedShip, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1));
    }

    public void DuplicateSelectedShips()
    {
        foreach (var selectedShip in SelectedShipList)
        {
            var newWrapper = selectedShip.Value with { Id = Guid.NewGuid() };
            FilteredShipList.Add(newWrapper.Id, newWrapper);
            if (PinnedShipList.ContainsKey(selectedShip.Key))
            {
                PinnedShipList.Add(newWrapper.Id, newWrapper);
            }
        }

        SetSelectAndPinAllButtonsStatus();
    }

    public void AddShip(object? obj)
    {
        if (obj is not Ship ship) return;

        var newWrapper = ShipBuildContainer.CreateNew(ship, null, null) with { ShipDataContainer =  GetShipDataContainer(ship) };
        FilteredShipList.Add(newWrapper.Id, newWrapper);
        PinnedShipList.Add(newWrapper.Id, newWrapper);

        SetSelectAndPinAllButtonsStatus();
        GetDataSectionsToDisplay();
        SearchString = string.Empty;
        ResearchedShip = string.Empty;
        SearchedShips.Clear();
    }

    private void SetSelectAndPinAllButtonsStatus() => SetSelectAndPinAllButtonsStatus(GetShipsToBeDisplayed());
    private void SetSelectAndPinAllButtonsStatus(IReadOnlyDictionary<Guid, ShipBuildContainer> list)
    {
        SelectAllShips = list.All(wrapper => SelectedShipList.ContainsKey(wrapper.Key));
        PinAllShips = list.All(wrapper => PinnedShipList.ContainsKey(wrapper.Key));
    }

    private ShipBuildContainer ResetBuild(ShipBuildContainer wrapper)
    {
        return wrapper with { Build = null, ActivatedConsumableSlots = null, SpecialAbilityActive = false, ShipDataContainer = GetShipDataContainer(wrapper.Ship), Modifiers = null };
    }

    public string ShipComparisonDataSectionToString(ShipComparisonDataSections dataSection)
    {
        return dataSection switch
        {
            ShipComparisonDataSections.General => "General",
            ShipComparisonDataSections.MainBattery => "MainBattery",
            ShipComparisonDataSections.He => "He",
            ShipComparisonDataSections.Ap => "Ap",
            ShipComparisonDataSections.Sap => "Sap",
            ShipComparisonDataSections.Torpedo => "Torpedo",
            ShipComparisonDataSections.SecondaryBattery => "SecondaryBattery",
            ShipComparisonDataSections.SecondaryBatteryShells => "SecondaryBatteryShells",
            ShipComparisonDataSections.AntiAir => "AntiAir",
            ShipComparisonDataSections.Asw => "Asw",
            ShipComparisonDataSections.AirStrike => "AirStrike",
            ShipComparisonDataSections.Maneuverability => "Maneuverability",
            ShipComparisonDataSections.Concealment => "Concealment",
            ShipComparisonDataSections.Survivability => "Survivability",
            ShipComparisonDataSections.RocketPlanes => "RocketPlanes",
            ShipComparisonDataSections.Rockets => "Rockets",
            ShipComparisonDataSections.TorpedoBombers => "TorpedoBombers",
            ShipComparisonDataSections.AerialTorpedoes => "AerialTorpedoes",
            ShipComparisonDataSections.Bombers => "Bombers",
            ShipComparisonDataSections.Bombs => "Bombs",
            ShipComparisonDataSections.Sonar => "Sonar",
            _ => dataSection.ToString(),
        };
    }
}
