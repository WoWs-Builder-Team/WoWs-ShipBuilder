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

        private int baseXp;
        private Account account;
        private int xpBonus;
        private int additionalXpBonus;
        private int capXpBonus;
        private int additionalCapXpBonus;
        private int fxpBonus;
        private int additionalFxpBonus;

        public int BaseXp
        {
            get { return baseXp; }
            set { baseXp = value; }
        }

        public Account AccountType
        {
            get { return account; }
            set { account = value; }
        }

        public double AccountTypeModifier
        {
            get { return AccountType == Account.Normal ? 1 : AccountType == Account.WGPremium ? 1.5 : 1.6; }
        }

        public int XpBonus
        {
            get { return xpBonus; }
            set { xpBonus = value; }
        }

        public int AdditionalXpBonus
        {
            get { return additionalXpBonus; }
            set { additionalXpBonus = value; }
        }

        public int CapXpBonus
        {
            get { return capXpBonus; }
            set { capXpBonus = value; }
        }

        public int AdditionalCapBonus
        {
            get { return additionalCapXpBonus; }
            set { additionalCapXpBonus = value; }
        }

        public int FxpBonus
        {
            get { return fxpBonus; }
            set { fxpBonus = value; }
        }

        public int AdditionalFxpBonus
        {
            get { return additionalFxpBonus; }
            set { additionalFxpBonus = value; }
        }

        public int XpResult
        {
            get
            {
                return (int)(BaseXp * AccountTypeModifier * (1 + ((XpBonus + AdditionalXpBonus) / 100)));
            }
        }

        public int CapXpResult
        {
            get
            {
                return (int)(XpResult + (BaseXp * AccountTypeModifier * ((CapXpBonus + additionalCapXpBonus) / 100)));
            }
        }

        public int FxpResult
        {
            get
            {
                return (int)(XpResult * 0.05 * (1 + ((FxpBonus + AdditionalFxpBonus) / 100)));
            }
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
