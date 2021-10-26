using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    public class ClassToPathConverter : IValueConverter
    {
        private const string CVIcon = "M 0 0 V 3.9688 H 8.7312 V 0 Z M 18.8417 4.715 10.675 0 V 9.43 Z M 0 5.5563 V 9.525 H 8.7312 V 5.5563 Z";
        private const string DDIcon = "m 0 0 v 9.525 L 17.9917 4.7625 Z";
        private const string BBIcon = "M 0 0 H 6.6146 L 1.5875 9.525 H 0 Z m 9.5406 0 h 1.3229 L 5.8365 9.525 H 4.5136 Z m 4.1167 0 l -5.0271 9.525 h 5.0271 l 8.599 -4.7625 z";
        private const string CCIcon = "M 0 0 H 8.2021 L 3.175 9.525 H 0 Z M 11.6573 0 6.6302 9.525 h 5.0271 l 8.599 -4.7625 z";
        private const string SubIcon = "m 0 0 h 3.175 V 9.7896 h -3.175 z M 5.8208 0.1719 V 9.6969 L 18.124 4.9344 Z";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ShipClass shipClass)
            {
                switch (shipClass)
                {
                    case ShipClass.Submarine:
                        return PathGeometry.Parse(SubIcon);
                    case ShipClass.Destroyer:
                        return PathGeometry.Parse(DDIcon);
                    case ShipClass.Cruiser:
                        return PathGeometry.Parse(CCIcon);
                    case ShipClass.Battleship:
                        return PathGeometry.Parse(BBIcon);
                    case ShipClass.AirCarrier:
                        return PathGeometry.Parse(CVIcon);
                    default:
                        return "";
                }
            }
            else if (value is ShipCategory category && parameter != null)
            {
                var par = parameter.ToString();
                if (par != null && par.Contains("stroke", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (category.Equals(ShipCategory.TechTree))
                    {
                        return Brushes.White;
                    }
                    else
                    {
                        return Brushes.Gold;
                    }
                }
                else
                {
                    if (category.Equals(ShipCategory.Premium) && par!.Contains("fill", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return Brushes.Gold;
                    }
                    else
                    {
                        return Brushes.White;
                    }
                }
            }

            return new BindingNotification(new NotSupportedException(), BindingErrorType.Error, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
