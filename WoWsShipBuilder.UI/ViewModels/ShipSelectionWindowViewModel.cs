using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class ShipSelectionWindowViewModel : ViewModelBase
    {
        public ShipSelectionWindowViewModel()
        {
            if (AppData.ShipSummaryList == null)
            {
                AppData.ShipSummaryList = AppDataHelper.Instance.GetShipSummaryList(AppData.Settings.SelectedServerType);
            }

            Test = new AutoCompleteFilterPredicate<string>((textSearch, shipName) => CultureInfo.CurrentCulture.CompareInfo.IndexOf(shipName, textSearch, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1);

            shipNameDictionary = AppData.ShipSummaryList.ToDictionary(ship => Localizer.Instance[$"{ship.Index}_FULL"].Localization, ship => ship);
            FilteredShipNameDictionary = new SortedDictionary<string, ShipSummary>(shipNameDictionary);
        }

        public AutoCompleteFilterPredicate<string> Test { get; }

        private bool tierFilterChecked = false;

        public bool TierFilterChecked
        {
            get => tierFilterChecked;
            set
            {
                this.RaiseAndSetIfChanged(ref tierFilterChecked, value);
                if (!value)
                {
                    SelectedTier = null;
                    ApplyFilter();
                }
            }
        }

        private bool classFilterChecked = false;

        public bool ClassFilterChecked
        {
            get => classFilterChecked;
            set
            {
                this.RaiseAndSetIfChanged(ref classFilterChecked, value);
                if (!value)
                {
                    SelectedClass = null;
                    ApplyFilter();
                }
            }
        }

        private bool nationFilterChecked = false;

        public bool NationFilterChecked
        {
            get => nationFilterChecked;
            set
            {
                this.RaiseAndSetIfChanged(ref nationFilterChecked, value);
                if (!value)
                {
                    SelectedNation = null;
                    ApplyFilter();
                }
            }
        }

        private bool typeFilterChecked = false;

        public bool TypeFilterChecked
        {
            get => typeFilterChecked;
            set
            {
                this.RaiseAndSetIfChanged(ref typeFilterChecked, value);
                if (!value)
                {
                    SelectedType = null;
                    ApplyFilter();
                }
            }
        }

        private List<string> tierList = new() { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };

        public List<string> TierList
        {
            get => tierList;
            set => this.RaiseAndSetIfChanged(ref tierList, value);
        }

        private List<string> classList = Enum.GetNames(typeof(ShipClass)).Select(shipClass => GetLocalizedString(shipClass)!).ToList();

        public List<string> ClassList
        {
            get => classList;
            set => this.RaiseAndSetIfChanged(ref classList, value);
        }

        private List<string> nationList = Enum.GetNames(typeof(Nation)).Select(nation => GetLocalizedString(nation)!).ToList();

        public List<string> NationList
        {
            get => nationList;
            set => this.RaiseAndSetIfChanged(ref nationList, value);
        }

        private List<string> typeList = Enum.GetNames(typeof(ShipCategory)).Select(shipType => GetLocalizedString(shipType)!).ToList();

        public List<string> TypeList
        {
            get => typeList;
            set => this.RaiseAndSetIfChanged(ref typeList, value);
        }

        private string? selectedTier;

        public string? SelectedTier
        {
            get => selectedTier;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedTier, value);
                ApplyFilter();
            }
        }

        private string? selectedClass;

        public string? SelectedClass
        {
            get => selectedClass;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedClass, value);
                ApplyFilter();
            }
        }

        private string? selectedNation;

        public string? SelectedNation
        {
            get => selectedNation;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedNation, value);
                ApplyFilter();
            }
        }

        private string? selectedType;

        public string? SelectedType
        {
            get => selectedType;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedType, value);
                ApplyFilter();
            }
        }

        private bool searchResult;

        public bool SearchResult
        {
            get => searchResult;
            set => this.RaiseAndSetIfChanged(ref searchResult, value);
        }

        private readonly Dictionary<string, ShipSummary> shipNameDictionary = new();
        private SortedDictionary<string, ShipSummary> filteredShipNameDictionary = new();

        public SortedDictionary<string, ShipSummary> FilteredShipNameDictionary
        {
            get => filteredShipNameDictionary;
            set => this.RaiseAndSetIfChanged(ref filteredShipNameDictionary, value);
        }

        private KeyValuePair<string, ShipSummary> selectedShip;

        public KeyValuePair<string, ShipSummary> SelectedShip
        {
            get => selectedShip;
            set => this.RaiseAndSetIfChanged(ref selectedShip, value);
        }

        private string? inputText;

        public string? InputText
        {
            get => inputText;
            set => this.RaiseAndSetIfChanged(ref inputText, value);
        }

        private static string? GetLocalizedString(string stringToLocalize)
        {
            return Translation.ResourceManager.GetString(stringToLocalize, Translation.Culture);
        }

        private void ApplyFilter()
        {
            var tmpDct = shipNameDictionary;
            
            if (SelectedTier != null)
            {
                tmpDct = tmpDct.Where(ship => ship.Value.Tier == tierList.IndexOf(selectedTier!) + 1).ToDictionary(ship => ship.Key, ship => ship.Value);
            }

            if (SelectedClass != null)
            {
                tmpDct = tmpDct.Where(ship => GetLocalizedString(ship.Value.ShipClass.ToString())!.Equals(selectedClass)).ToDictionary(ship => ship.Key, ship => ship.Value);
            }

            if (SelectedNation != null)
            {
                tmpDct = tmpDct.Where(ship => GetLocalizedString(ship.Value.Nation.ToString())!.Equals(selectedNation)).ToDictionary(ship => ship.Key, ship => ship.Value);
            }

            if (SelectedType != null)
            {
                tmpDct = tmpDct.Where(ship => GetLocalizedString(ship.Value.Category.ToString())!.Equals(selectedType)).ToDictionary(ship => ship.Key, ship => ship.Value);
            }

            FilteredShipNameDictionary = new SortedDictionary<string, ShipSummary>(tmpDct);

            if (FilteredShipNameDictionary.Count == 0)
            {
                SearchResult = true;
            }
            else
            {
                SearchResult = false;
            }
        }

        public void UpdateResult()
        {
            if (!string.IsNullOrEmpty(InputText))
            {
                bool tmp = true;
                foreach (var key in FilteredShipNameDictionary.Keys)
                {
                    if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(key, InputText.Trim(), CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1)
                    {
                        tmp = false;
                        break;
                    }
                }

                SearchResult = tmp;
            }
        }
    }
}
