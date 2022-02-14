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

namespace WoWsShipBuilder.UI.ViewModels
{
    public class CalculatorViewModel : ViewModelBase
    {
        public ObservableCollection<ExteriorUI> Exterior { get; set; }

        public CalculatorViewModel()
        {
            Exterior = new((IEnumerable<ExteriorUI>)AppDataHelper.Instance.GetCommonExterior());
        }

        private void GetShipPermoFlages(Ship ship)
        {
            Exterior.AddRange((IEnumerable<ExteriorUI>)AppDataHelper.Instance.GetShipExterior(ship.Permaflages));
        }
    }
}

// call appdata to get data here and abobe collection
// make View
