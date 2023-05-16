using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Desktop.Converters;

public class WeaponTypeToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string b)
        {
            return b.Equals(ProjectileType.DepthCharge.ToString());
        }

        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error, value);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
