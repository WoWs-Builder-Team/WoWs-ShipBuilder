using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.Other;

public class ShipComparisonViewModel : ViewModelBase
{
    #region Properties

    private List<Ship?> fullShipList = new(AppData.ShipDictionary!.Values);
    public List<Ship?> FullShipList
    {
        get => fullShipList;
        set => this.RaiseAndSetIfChanged(ref fullShipList, value);
    }

    private List<Ship> filteredShipList = new(AppData.ShipDictionary!.Values!);
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

    private Dictionary<string, bool> tierList = new(){{"I", false}, {"II", false}, {"III", false}, {"IV", false}, {"V", false}, {"VI", false}, {"VII", false}, {"VIII", false}, {"IX", false}, {"X", false}, {"XI", false}};
    public Dictionary<string, bool> TierList
    {
        get => tierList;
        set
        {
            FilteredShipList = ApplyFilters();
            this.RaiseAndSetIfChanged(ref tierList, value);
        }
    }

    private Dictionary<ShipClass, bool> classList = Enum.GetValues<ShipClass>().Except(new List<ShipClass> { ShipClass.Auxiliary }).ToDictionary(key => key, _ => false);
    public Dictionary<ShipClass, bool> ClassList
    {
        get => classList;
        set
        {
            FilteredShipList = ApplyFilters();
            this.RaiseAndSetIfChanged(ref classList, value);
        }
    }

    private Dictionary<Nation, bool> nationList = Enum.GetValues<Nation>().Except(new List<Nation> {Nation.Common}).ToDictionary(key => key, _ => false);
    public Dictionary<Nation, bool> NationList
    {
        get => nationList;
        set
        {
            FilteredShipList = ApplyFilters();
            this.RaiseAndSetIfChanged(ref nationList, value);
        }
    }

    private Dictionary<ShipCategory, bool> categoryList = Enum.GetValues<ShipCategory>().Except(new List<ShipCategory> {ShipCategory.Disabled, ShipCategory.Clan}).ToDictionary(key => key, _ => false);
    public Dictionary<ShipCategory, bool> CategoryList
    {
        get => categoryList;
        set
        {
            FilteredShipList = ApplyFilters();
            this.RaiseAndSetIfChanged(ref categoryList, value);
        }
    }

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
        return FullShipList.Where(data => data is not null &&
                                          TierList[TierList.First(x => data.Tier == TierList.IndexOf(x) + 1).Key] &&
                                          ClassList[ClassList.First(x => data.ShipClass.Equals(x.Key)).Key] &&
                                          NationList[NationList.First(x => data.ShipNation.Equals(x.Key)).Key] &&
                                          CategoryList[CategoryList.First(x => data.ShipCategory.Equals(x.Key)).Key]).ToList()!;
    }

    public void ToggleShowFilteredOnly()
    {
        ShowFilteredOnly = !ShowFilteredOnly;
    }

    public void ToggleTierSelection(string value)
    {
        TierList[value] = !TierList[value];
    }

    public void ToggleClassSelection(ShipClass value)
    {
        ClassList[value] = !ClassList[value];
    }

    public void ToggleNationSelection(Nation value)
    {
        NationList[value] = !NationList[value];
    }

    public void ToggleCategorySelection(ShipCategory value)
    {
        CategoryList[value] = !CategoryList[value];
    }

    public void ToggleAllTiers(bool b)
    {
        foreach (var tier in TierList)
        {
            TierList[tier.Key] = b;
        }
    }

    public void ToggleAllClasses(bool b)
    {
        foreach (var shipClass in ClassList)
        {
            ClassList[shipClass.Key] = b;
        }
    }

    public void ToggleAllNations(bool b)
    {
        foreach (var nation in NationList)
        {
            NationList[nation.Key] = b;
        }
    }

    public void ToggleAllCategories(bool b)
    {
        foreach (var category in CategoryList)
        {
            CategoryList[category.Key] = b;
        }
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
    }

    #endregion
}
