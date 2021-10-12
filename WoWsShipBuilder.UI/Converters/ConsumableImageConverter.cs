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
    public class ConsumableImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Consumable consumable)
            {
                if (value is not (ShipConsumable, Consumable tmpConsumable))
                {
                    throw new NotSupportedException();
                }

                consumable = tmpConsumable;
            }

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

            string iconName = string.IsNullOrEmpty(consumable.IconId) ? consumable.Name : consumable.IconId;
            Uri uri = new($"avares://{assemblyName}/Assets/consumable_icons/consumable_{iconName}.png");

            Stream? asset = assets.Open(uri);
            return new Bitmap(asset);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
