using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class CalculatorViewModel : ViewModelBase
    {
        public ObservableCollection<ExteriorUI> Exterior { get; set; }

        public CalculatorViewModel()
        {
            Exterior = new((IEnumerable<ExteriorUI>)DesktopAppDataService.Instance.GetCommonExterior());
        }

        private void GetShipPermoFlages(Ship ship)
        {
            Exterior.AddRange((IEnumerable<ExteriorUI>)DesktopAppDataService.Instance.GetShipExterior(ship.Permoflages));
        }
    }

    public enum Account
    {
        Normal,
        WGPremium,
        WoWsPremium,
    }
}

// call appdata to get data here and abobe collection
// make View
