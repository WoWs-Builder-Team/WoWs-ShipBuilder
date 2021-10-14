using System;
using System.Collections.Generic;
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
    public class ModuleImageConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is not ShipUpgrade upgrade || values[1] is not bool isSelected || !targetType.IsAssignableFrom(typeof(Bitmap)))
            {
                throw new NotSupportedException();
            }

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var baseUri = $"avares://{assemblyName}/Assets/modules/";
            string uriSuffix = isSelected ? "_installed" : string.Empty;

            var uri = new Uri($"{baseUri}icon_module_{upgrade.UcType.ToString()}{uriSuffix}.png");
            Stream? asset;
            try
            {
                asset = assets.Open(uri);
            }
            catch (FileNotFoundException)
            {
                asset = null;
            }

            asset ??= assets.Open(new Uri($"{baseUri}placeholder.png"));
            return new Bitmap(asset);
        }
    }
}
