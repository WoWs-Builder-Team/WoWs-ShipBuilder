using System;
using WoWsShipBuilder.Core.DataUI.UnitTranslations;

namespace WoWsShipBuilder.Core.DataUI
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataUiUnitAttribute : Attribute
    {
        public DataUiUnitAttribute(string unitKey)
        {
            UnitKey = unitKey.Replace("Unit_", string.Empty);
        }

        public string UnitKey { get; }

        public string Localization
        {
            get
            {
                string? localization = UnitLocalization.ResourceManager.GetString($"Unit_{UnitKey}");
                return localization != null ? $" {localization}" : string.Empty;
            }
        }
    }
}
