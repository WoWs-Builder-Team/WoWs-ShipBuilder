using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;
using WoWsShipBuilder.ViewModels.Helper;

namespace WoWsShipBuilder.ViewModels.Other;

public class ShipComparisonViewModel : ViewModelBase
{
    #region Properties

    private List<Ship> fullShipList = new(AppData.ShipDictionary!.Values);

    public List<Ship> FullShipList
    {
        get => fullShipList;
        set => this.RaiseAndSetIfChanged(ref fullShipList, value);
    }

    private List<Ship> filteredShipList = new();

    public List<Ship> FilteredShipList
    {
        get => filteredShipList;
        set => this.RaiseAndSetIfChanged(ref filteredShipList, value);
    }

    private List<Ship> selectedShipList = new();

    public List<Ship> SelectedShipList
    {
        get => selectedShipList;
        set => this.RaiseAndSetIfChanged(ref selectedShipList, value);
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

    public ObservableCollection<BuildVmCollection> CustomBuilds { get; } = new();

    public IEnumerable<ShipClass> AvailableClasses { get; } = Enum.GetValues<ShipClass>().Except(new [] { ShipClass.Auxiliary });

    public IEnumerable<Nation> AvailableNations { get; } = Enum.GetValues<Nation>().Except(new [] { Nation.Common });

    public IEnumerable<ShipCategory> AvailableShipCategories { get; } = Enum.GetValues<ShipCategory>().Except(new [] { ShipCategory.Disabled, ShipCategory.Clan });

    private string searchString = default!;

    public string SearchString
    {
        get => searchString;
        set => this.RaiseAndSetIfChanged(ref searchString, value);
    }

    private bool showFilteredOnly;

    public bool ShowFilteredOnly
    {
        get => showFilteredOnly;
        set
        {
            GetShipsToBeDisplayedDictionary();
            this.RaiseAndSetIfChanged(ref showFilteredOnly, value);
        }
    }

    #endregion

    #region Methods

    private List<Ship> ApplyFilters()
    {
        return FullShipList
            .Where(data => SelectedTiers.Contains(data.Tier) &&
                           SelectedClasses.Contains(data.ShipClass) &&
                           SelectedNations.Contains(data.ShipNation) &&
                           SelectedCategories.Contains(data.ShipCategory))
            .ToList();
    }

    public void ToggleShowFilteredOnly()
    {
        ShowFilteredOnly = !ShowFilteredOnly;
    }

    public void ToggleTierSelection(int value)
    {
        if (SelectedTiers.Contains(value))
        {
            SelectedTiers.Remove(value);
        }
        else
        {
            SelectedTiers.Add(value);
        }

        FilteredShipList = ApplyFilters();
    }

    public void ToggleClassSelection(ShipClass value)
    {
        if (SelectedClasses.Contains(value))
        {
            SelectedClasses.Remove(value);
        }
        else
        {
            SelectedClasses.Add(value);
        }

        FilteredShipList = ApplyFilters();
    }

    public void ToggleNationSelection(Nation value)
    {
        if (SelectedNations.Contains(value))
        {
            SelectedNations.Remove(value);
        }
        else
        {
            SelectedNations.Add(value);
        }

        FilteredShipList = ApplyFilters();
    }

    public void ToggleCategorySelection(ShipCategory value)
    {
        if (SelectedCategories.Contains(value))
        {
            SelectedCategories.Remove(value);
        }
        else
        {
            SelectedCategories.Add(value);
        }

        FilteredShipList = ApplyFilters();
    }

    public void ToggleAllTiers(bool activationState)
    {
        SelectedTiers.Clear();
        if (activationState)
        {
            SelectedTiers.AddRange(Enumerable.Range(1, 11));
        }

        FilteredShipList = ApplyFilters();
    }

    public void ToggleAllClasses(bool activationState)
    {
        SelectedClasses.Clear();
        if (activationState)
        {
            SelectedClasses.AddRange(Enum.GetValues<ShipClass>().Except(new[] { ShipClass.Auxiliary }));
        }

        FilteredShipList = ApplyFilters();
    }

    public void ToggleAllNations(bool activationState)
    {
        SelectedNations.Clear();
        if (activationState)
        {
            SelectedNations.AddRange(Enum.GetValues<Nation>().Except(new[] { Nation.Common }));
        }

        FilteredShipList = ApplyFilters();
    }

    public void ToggleAllCategories(bool activationState)
    {
        SelectedCategories.Clear();
        if (activationState)
        {
            SelectedCategories.AddRange(Enum.GetValues<ShipCategory>().Except(new[] { ShipCategory.Disabled, ShipCategory.Clan }));
        }

        FilteredShipList = ApplyFilters();
    }

    public List<Ship> GetShipsToBeDisplayedDictionary()
    {
        return ShowFilteredOnly ? selectedShipList : FilteredShipList;
    }

    public void SelectShip(Ship ship)
    {
        if (SelectedShipList.Contains(ship))
        {
            RemoveSelectedShip(ship);
        }
        else
        {
            SelectedShipList.Add(ship);
        }
    }

    public void RemoveSelectedShip(Ship ship)
    {
        SelectedShipList.Remove(ship);
        RemoveBuild(ship.Index);
    }

    public void AddBuild(BuildVmCollection build)
    {
        RemoveBuild(build.ShipIndex);
        CustomBuilds.Add(build);
    }

    public void RemoveBuild(string shipIndex)
    {
        if (ContainsBuild(shipIndex))
        {
            CustomBuilds.Remove(CustomBuilds.First(x=> x.ShipIndex.Equals(shipIndex)));
        }
    }

    public bool ContainsBuild(string shipIndex)
    {
        return CustomBuilds.Select(x => x.ShipIndex).Contains(shipIndex);
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

    #endregion

    public void ToggleDataSection(DataSections dataSection)
    {
        selectedDataSection = dataSection;
    }
}
