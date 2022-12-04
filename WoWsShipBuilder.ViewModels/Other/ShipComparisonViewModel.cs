using System;
using System.Collections.ObjectModel;
using System.Globalization;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.Helper;

namespace WoWsShipBuilder.ViewModels.Other;

public partial class ShipComparisonViewModel : ViewModelBase
{
    public const string DataNotAvailable = "N/A";

    public static readonly string DefaultBuildName = "---";

    private readonly ILocalizer localizer;

    private readonly AppSettings appSettings;

    private readonly IEnumerable<Ship> fullShipList = AppData.ShipDictionary.Values;

    private List<ShipComparisonDataWrapper> filteredShipList = new();

    private List<ShipComparisonDataWrapper> FilteredShipList
    {
        get => filteredShipList;
        set => this.RaiseAndSetIfChanged(ref filteredShipList, value);
    }

    public List<ShipComparisonDataWrapper> SelectedShipList { get; } = new();

    public List<ShipComparisonDataWrapper> PinnedShipList { get; } = new();

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

    private readonly List<ShipComparisonDataWrapper> wrappersCache = new();

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

        list.AddRange(InitialiseShipComparisonDataWrapper(fullShipList.Where(data => !ContainsShipIndex(data.Index, filteredShips) &&
                                                                                                     !ContainsShipIndex(data.Index, cachedWrappers) &&
                                                                                                     SelectedTiers.Contains(data.Tier) &&
                                                                                                     SelectedClasses.Contains(data.ShipClass) &&
                                                                                                     SelectedNations.Contains(data.ShipNation) &&
                                                                                                     SelectedCategories.Contains(data.ShipCategory)).ToList()));

        cachedWrappers.ForEach(x => wrappersCache.Remove(x));
        filteredShips.ForEach(x => FilteredShipList.Remove(x));
        FilteredShipList.Where(x => ContainsWrapper(x, SelectedShipList) && !ContainsWrapper(x, PinnedShipList)).ToList().ForEach(x => SelectedShipList.Remove(x));
        wrappersCache.AddRange(FilteredShipList.Where(x => !ContainsWrapper(x, wrappersCache)));

        SetSelectAndPinAllButtonsStatus(list);

        FilteredShipList = list;

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

    public List<ShipComparisonDataWrapper> GetShipsToBeDisplayedList()
    {
        return GetShipsToBeDisplayedList(false);
    }

    private List<ShipComparisonDataWrapper> GetShipsToBeDisplayedList(bool disableHideShipsIfNoSelectedSection)
    {
        List<ShipComparisonDataWrapper> list = ShowPinnedShipsOnly ? PinnedShipList : FilteredShipList;

        if (!disableHideShipsIfNoSelectedSection)
        {
            list = HideShipsIfNoSelectedSection(list);
        }

        return list;
    }

    public void EditBuilds(List<ShipComparisonDataWrapper> newWrappers)
    {
        EditBuilds(newWrappers, false);
    }

    private void EditBuilds(List<ShipComparisonDataWrapper> newWrappers, bool clearCache)
    {
        foreach (var wrapper in newWrappers)
        {
            if (ContainsWrapper(wrapper, FilteredShipList))
            {
                FilteredShipList.Replace(SelectWrapper(wrapper, FilteredShipList), wrapper);
            }
            else
            {
                FilteredShipList.Add(wrapper);
            }

            if (ContainsWrapper(wrapper, PinnedShipList))
            {
                PinnedShipList.Replace(SelectWrapper(wrapper, PinnedShipList), wrapper);
            }
            else if (ContainsShipIndex(wrapper.Ship.Index, PinnedShipList))
            {
                PinnedShipList.Add(wrapper);
            }

            if (!clearCache && ContainsWrapper(wrapper, wrappersCache))
            {
                wrappersCache.Replace(SelectWrapper(wrapper, wrappersCache), wrapper);
            }

            if (ContainsWrapper(wrapper, SelectedShipList))
            {
                SelectedShipList.Replace(SelectWrapper(wrapper, SelectedShipList), wrapper);
            }
        }

        if (clearCache)
        {
            wrappersCache.Clear();
        }
    }

    public List<ShipComparisonDataWrapper> RemoveBuilds(IEnumerable<ShipComparisonDataWrapper> builds)
    {
        List<ShipComparisonDataWrapper> warnings = new();
        List<ShipComparisonDataWrapper> buildList = new(builds);
        foreach (var wrapper in buildList)
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
                    ShipComparisonDataWrapper err = new(wrapper.Ship, GetShipConfiguration(wrapper.Ship), null, wrapper.Id);
                    EditBuilds(new() { err });
                    warnings.Add(err);
                }
                else
                {
                    warnings.Add(wrapper);
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
        IEnumerable<ShipComparisonDataWrapper> list = FilteredShipList.Where(x => x.Build is not null).Select(x => new ShipComparisonDataWrapper(x.Ship, GetShipConfiguration(x.Ship), null, x.Id));
        EditBuilds(list.ToList(), true);
        SetSelectAndPinAllButtonsStatus();
    }

    public async Task AddPinnedShip(ShipComparisonDataWrapper wrapper)
    {
        if (!ContainsWrapper(wrapper, PinnedShipList))
        {
            PinnedShipList.Add(wrapper);
        }
        else
        {
            await RemovePinnedShip(wrapper);
        }

        SetSelectAndPinAllButtonsStatus();
    }

    private async Task RemovePinnedShip(ShipComparisonDataWrapper wrapper)
    {
        PinnedShipList.Remove(wrapper);
        await ApplyFilters();
    }

    public void AddSelectedShip(ShipComparisonDataWrapper wrapper)
    {
        if (!ContainsWrapper(wrapper, SelectedShipList))
        {
            SelectedShipList.Add(wrapper);
        }
        else
        {
            RemoveSelectedShip(wrapper);
        }

        SetSelectAndPinAllButtonsStatus();
    }

    private void RemoveSelectedShip(ShipComparisonDataWrapper wrapper)
    {
        SelectedShipList.Remove(wrapper);
    }

    public static bool ContainsWrapper(ShipComparisonDataWrapper wrapper, IEnumerable<ShipComparisonDataWrapper> list)
    {
        return list.Select(x => x.Id).Contains(wrapper.Id);
    }

    private static bool ContainsShipIndex(string shipIndex, IEnumerable<ShipComparisonDataWrapper> list)
    {
        return list.Select(x => x.Ship.Index).Contains(shipIndex);
    }

    private static ShipComparisonDataWrapper SelectWrapper(ShipComparisonDataWrapper wrapper, IEnumerable<ShipComparisonDataWrapper> list)
    {
        return list.First(x => x.Id.Equals(wrapper.Id));
    }

    private List<ShipComparisonDataWrapper> InitialiseShipComparisonDataWrapper(List<Ship> ships)
    {
        List<ShipComparisonDataWrapper> list = new();
        foreach (var ship in ships)
        {
            list.Add(new(ship, GetShipConfiguration(ship)));
        }

        return list;
    }

    private void ChangeModulesBatch()
    {
        IEnumerable<ShipComparisonDataWrapper> list = FilteredShipList.Where(x => x.Build is null).Select(x => new ShipComparisonDataWrapper(x.Ship, GetShipConfiguration(x.Ship), null, x.Id));
        EditBuilds(list.ToList());
    }

    private ShipDataContainer GetShipConfiguration(Ship ship)
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
        return ShipDataContainer.CreateFromShip(ship, shipConfiguration, new());
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

    private List<ShipComparisonDataWrapper> HideShipsIfNoSelectedSection(IEnumerable<ShipComparisonDataWrapper> list)
    {
        if (!HideShipsWithoutSelectedSection)
        {
            return list.ToList();
        }

        List<ShipComparisonDataWrapper> newList = SelectedDataSection switch
        {
            ShipComparisonDataSections.MainBattery => list.Where(x => x.ShipDataContainer.MainBatteryDataContainer is not null).ToList(),
            ShipComparisonDataSections.He => list.Where(x => x.HeDamage is not null).ToList(),
            ShipComparisonDataSections.Ap => list.Where(x => x.ApDamage is not null).ToList(),
            ShipComparisonDataSections.Sap => list.Where(x => x.SapDamage is not null).ToList(),
            ShipComparisonDataSections.Torpedo => list.Where(x => x.ShipDataContainer.TorpedoArmamentDataContainer is not null).ToList(),
            ShipComparisonDataSections.RocketPlanes => list.Where(x => x.RocketPlanesType.Any()).ToList(),
            ShipComparisonDataSections.Rockets => list.Where(x => x.RocketPlanesWeaponType.Any()).ToList(),
            ShipComparisonDataSections.TorpedoBombers => list.Where(x => x.TorpedoBombersType.Any()).ToList(),
            ShipComparisonDataSections.AerialTorpedoes => list.Where(x => x.TorpedoBombersWeaponType.Any()).ToList(),
            ShipComparisonDataSections.Bombers => list.Where(x => x.BombersType.Any()).ToList(),
            ShipComparisonDataSections.Bombs => list.Where(x => x.BombersWeaponType.Any()).ToList(),
            ShipComparisonDataSections.Sonar => list.Where(x => x.ShipDataContainer.PingerGunDataContainer is not null).ToList(),
            ShipComparisonDataSections.SecondaryBattery => list.Where(x => x.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries is not null).ToList(),
            ShipComparisonDataSections.SecondaryBatteryShells => list.Where(x => x.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries is not null).ToList(),
            ShipComparisonDataSections.AntiAir => list.Where(x => x.ShipDataContainer.AntiAirDataContainer is not null).ToList(),
            ShipComparisonDataSections.AirStrike => list.Where(x => x.ShipDataContainer.AirstrikeDataContainer is not null).ToList(),
            ShipComparisonDataSections.Asw => list.Where(x => x.ShipDataContainer.AswAirstrikeDataContainer is not null || x.ShipDataContainer.DepthChargeLauncherDataContainer is not null).ToList(),
            _ => list.ToList(),
        };

        newList.AddRange(PinnedShipList.Where(x => !ContainsWrapper(x, newList)));

        return newList;
    }

    private void GetDataSectionsToDisplay()
    {
        List<ShipComparisonDataSections> dataSections = Enum.GetValues<ShipComparisonDataSections>().ToList();
        List<ShipComparisonDataWrapper> shipList = GetShipsToBeDisplayedList(true);

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
                        if (!shipList.Any(x => x.ShipDataContainer.MainBatteryDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.He:
                        if (!shipList.Any(x => x.HeDamage is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.Ap:
                        if (!shipList.Any(x => x.ApDamage is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.Sap:
                        if (!shipList.Any(x => x.SapDamage is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.Torpedo:
                        if (!shipList.Any(x => x.ShipDataContainer.TorpedoArmamentDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.SecondaryBattery:
                    case ShipComparisonDataSections.SecondaryBatteryShells:
                        if (!shipList.Any(x => x.ShipDataContainer.SecondaryBatteryUiDataContainer.Secondaries is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.AntiAir:
                        if (!shipList.Any(x => x.ShipDataContainer.AntiAirDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.Asw:
                        if (!shipList.Any(x => x.ShipDataContainer.AswAirstrikeDataContainer is not null || x.ShipDataContainer.DepthChargeLauncherDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.AirStrike:
                        if (!shipList.Any(x => x.ShipDataContainer.AirstrikeDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.RocketPlanes:
                        if (!shipList.Any(x => x.RocketPlanesType.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.Rockets:
                        if (!shipList.Any(x => x.RocketPlanesWeaponType.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.TorpedoBombers:
                        if (!shipList.Any(x => x.TorpedoBombersType.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.AerialTorpedoes:
                        if (!shipList.Any(x => x.TorpedoBombersWeaponType.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.Bombers:
                        if (!shipList.Any(x => x.BombersType.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.Bombs:
                        if (!shipList.Any(x => x.BombersWeaponType.Any()))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;

                    case ShipComparisonDataSections.Sonar:
                        if (!shipList.Any(x => x.ShipDataContainer.PingerGunDataContainer is not null))
                        {
                            dataSections.Remove(dataSection);
                        }

                        break;
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

    public async Task PinAllDisplayedShips()
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
            ShipComparisonDataWrapper newWrapper = new(selectedShip.Ship, selectedShip.ShipDataContainer, selectedShip.Build);
            FilteredShipList.Add(newWrapper);
            if (ContainsWrapper(selectedShip, PinnedShipList))
            {
                PinnedShipList.Add(newWrapper);
            }
        }

        SetSelectAndPinAllButtonsStatus();
    }

    public void AddShip(object? obj)
    {
        if (obj is not Ship ship) return;

        ShipComparisonDataWrapper newWrapper = new(ship, GetShipConfiguration(ship));
        FilteredShipList.Add(newWrapper);
        PinnedShipList.Add(newWrapper);

        SetSelectAndPinAllButtonsStatus();
        GetDataSectionsToDisplay();
        SearchString = string.Empty;
        ResearchedShip = string.Empty;
        SearchedShips.Clear();
    }

    private void SetSelectAndPinAllButtonsStatus() => SetSelectAndPinAllButtonsStatus(GetShipsToBeDisplayedList());
    private void SetSelectAndPinAllButtonsStatus(IReadOnlyCollection<ShipComparisonDataWrapper> list)
    {
        SelectAllShips = list.Where(wrapper => !ContainsWrapper(wrapper, SelectedShipList)).ToList().Count == 0;
        PinAllShips = list.Where(wrapper => !ContainsWrapper(wrapper, PinnedShipList)).ToList().Count == 0;
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
