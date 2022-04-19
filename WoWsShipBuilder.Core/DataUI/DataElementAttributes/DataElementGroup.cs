using System;
using WoWsShipBuilder.Core.Translations;

namespace WoWsShipBuilder.Core.DataUI.DataElementAttributes;

[AttributeUsage(AttributeTargets.Property)]
public class DataElementGroup : Attribute
{
    public DataElementGroup(string groupKey)
    {
        GroupKey = groupKey;
    }

    public string GroupKey { get;}

    public string Localization
    {
        get
        {
            string? localization = Translation.ResourceManager.GetString(GroupKey);
            return localization != null ? $" {localization}" : string.Empty;
        }
    }
}
