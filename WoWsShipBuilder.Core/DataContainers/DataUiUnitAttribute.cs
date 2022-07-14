using System;
using WoWsShipBuilder.Core.Localization;

namespace WoWsShipBuilder.Core.DataContainers
{
    [Obsolete("Usage replaced by DataElementTypeAttribute and source generation.", true)]
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
                string? localization = Translation.ResourceManager.GetString($"Unit_{UnitKey}");
                return localization != null ? $" {localization}" : string.Empty;
            }
        }
    }
}
