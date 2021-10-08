using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    public class ModernizationImageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            value ??= DataHelper.PlaceholderModernization;

            if (value is not Modernization modernization || !targetType.IsAssignableFrom(typeof(Bitmap)))
            {
                throw new NotSupportedException();
            }

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            Uri uri;
            if (modernization.Index != null)
            {
                uri = new Uri($"avares://{assemblyName}/Assets/modernization_icons/icon_modernization_{modernization.Name}.png");
            }
            else
            {
                uri = new Uri($"avares://{assemblyName}/Assets/modernization_icons/modernization_default3.png");
            }

            Stream? asset = assets.Open(uri);
            return new Bitmap(asset);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
