using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using WoWsShipBuilder.DataContainers;
using WoWsShipBuilder.Desktop.Settings;
using WoWsShipBuilder.Infrastructure.Localization;

namespace WoWsShipBuilder.Desktop.Converters;

public class ModifierConverter : IMultiValueConverter
{
    private readonly ILocalizer localizer;

    public ModifierConverter()
    {
        localizer = AppSettingsHelper.LocalizerInstance;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1123:Do not place regions within elements",
        Justification = "<The code is a fucking mess otherwise>")]
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var returnFilter = ReturnFilter.All;
        if (parameter is string stringParam)
        {
            if (stringParam.Contains("desc", StringComparison.InvariantCultureIgnoreCase))
            {
                returnFilter = ReturnFilter.Description;
            }
            else if (stringParam.Contains("value", StringComparison.InvariantCultureIgnoreCase))
            {
                returnFilter = ReturnFilter.Value;
            }
        }

        float modifier = 0;
        var modifierInitialized = false;

        switch (values[1])
        {
            case float floatMod:
                modifier = floatMod;
                modifierInitialized = true;
                break;
            case double doubleMod:
                modifier = (float)doubleMod;
                modifierInitialized = true;
                break;
        }

        if (values[0] is string localizerKey && modifierInitialized)
        {
            return ModifierProcessor.GetUiModifierString(localizerKey, modifier, returnFilter, localizer);
        }

        return string.Empty;
    }
}
