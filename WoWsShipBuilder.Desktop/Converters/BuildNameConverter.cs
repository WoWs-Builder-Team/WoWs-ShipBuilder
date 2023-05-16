using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Desktop.Settings;
using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Desktop.Converters;

public class BuildNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Build build)
        {
            return string.IsNullOrWhiteSpace(build.ShipIndex) ? build.BuildName : build.BuildName + " - " + AppSettingsHelper.LocalizerInstance[build.ShipIndex].Localization;
        }

        return new BindingNotification(new NotSupportedException());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotSupportedException());
    }
}
