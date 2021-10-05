using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;

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

            ShipNameList = AppData.ShipSummaryList.Select(ship => Localizer.Instance[ship.Index]).ToList();
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

        private List<string> shipNameList = new();

        public List<string> ShipNameList
        {
            get => shipNameList;
            set => this.RaiseAndSetIfChanged(ref shipNameList, value);
        }
    }
}
