using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using WoWsShipBuilder.Common.Infrastructure;
using WoWsShipBuilder.Common.Infrastructure.GameData;
using WoWsShipBuilder.DataStructures;
using Brushes = Avalonia.Media.Brushes;

namespace WoWsShipBuilder.UI.Converters
{
    public class ClassToPathConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            switch (value)
            {
                case ShipClass shipClass:
                    return PathGeometry.Parse(ClassToPathHelper.GetSvgPathFromClass(shipClass));

                case ShipCategory category when parameter != null:
                {
                    var par = parameter.ToString();
                    bool border = par!.Contains("stroke", StringComparison.InvariantCultureIgnoreCase);

                    var color = Color.Parse(ClassToPathHelper.GetColorFromCategory(category, border));
                    return new SolidColorBrush(color);
                }

                default:
                    return new BindingNotification(new NotSupportedException(), BindingErrorType.Error, value);
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
