using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
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

            TierList.AddRange(new[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" });

            PlaceholderTier = "I ~ X";

            var unsortedShipNameDictionary = AppData.ShipSummaryList.ToDictionary(ship => Localizer.Instance[$"{ship.Index}_FULL"], ship => ship.Index);
            ShipNameDictionary = new SortedDictionary<string, string>(unsortedShipNameDictionary);
        }

        private List<string> tierList = new();

        public List<string> TierList
        {
            get => tierList;
            set => this.RaiseAndSetIfChanged(ref tierList, value);
        }

        private string placeholderTier = "";

        public string PlaceholderTier
        {
            get => placeholderTier;
            set => this.RaiseAndSetIfChanged(ref placeholderTier, value);
        }

        private string? selectedTier;

        public string? SelectedTier
        {
            get => selectedTier;
            set => this.RaiseAndSetIfChanged(ref selectedTier, value);
        }

        private SortedDictionary<string, string> shipNameDictionary = new();

        public SortedDictionary<string, string> ShipNameDictionary
        {
            get => shipNameDictionary;
            set => this.RaiseAndSetIfChanged(ref shipNameDictionary, value);
        }

        private KeyValuePair<string, string> selectedShip;

        public KeyValuePair<string, string> SelectedShip
        {
            get => selectedShip;
            set => this.RaiseAndSetIfChanged(ref selectedShip, value);
        }
    }
}
