using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NLog;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    /// <summary>
    /// Multi-purpose converter to convert various types to an image.
    /// </summary>
    class ImagePathConverter : IValueConverter
    {
        private static readonly Logger Logger = Logging.GetLogger("ImagePathConverter");

        private readonly IFileSystem fileSystem;

        public ImagePathConverter()
            : this(new FileSystem())
        {
        }

        internal ImagePathConverter(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        [SuppressMessage("Spacing Rules", "SA1008", Justification = "StyleCop error results in false positive.")]
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;

            switch (value)
            {
                case string imageName when parameter is string type:
                {
                    Logger.Debug("Processing regular image name.");
                    Uri? uri = null;

                    if (imageName.Equals("blank"))
                    {
                        uri = new Uri($"avares://{assemblyName}/Assets/blank.png");
                    }
                    else if (type.Equals("skill", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageName = GetSkillimageId(imageName).ToLower();
                        uri = new Uri($"avares://{assemblyName}/Assets/Skills/{imageName}.png");
                    }
                    else if (type.Equals("ship", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string? imagePath = fileSystem.Path.Combine(AppDataHelper.Instance.AppDataImageDirectory, "Ships", $"{imageName}.png");
                        Stream stream;
                        if (fileSystem.File.Exists(imagePath))
                        {
                            stream = fileSystem.File.OpenRead(imagePath);
                        }
                        else
                        {
                            Logger.Warn("Using fallback error icon because image '{}' was not found.", imagePath);
                            stream = LoadErrorIcon(assets);
                        }

                        return new Bitmap(stream);
                    }

                    Stream asset = LoadEmbeddedAsset(assets, uri);
                    return new Bitmap(asset);
                }

                case Modernization modernization:
                {
                    Logger.Debug("Processing modernization.");
                    Uri uri;
                    if (modernization.Index != null)
                    {
                        uri = new($"avares://{assemblyName}/Assets/modernization_icons/icon_modernization_{modernization.Name}.png");
                    }
                    else
                    {
                        uri = new($"avares://{assemblyName}/Assets/modernization_icons/modernization_default3.png");
                    }

                    Stream asset = LoadEmbeddedAsset(assets, uri);
                    return new Bitmap(asset);
                }

                case Consumable or (ShipConsumable, Consumable):
                {
                    Logger.Debug("Processing consumable.");
                    if (value is not Consumable consumable)
                    {
                        consumable = (((ShipConsumable, Consumable tmpConsumable))value).tmpConsumable;
                    }

                    string iconName = string.IsNullOrEmpty(consumable.IconId) ? consumable.Name : consumable.IconId;
                    Uri uri = new($"avares://{assemblyName}/Assets/consumable_icons/consumable_{iconName}.png");

                    Stream asset = LoadEmbeddedAsset(assets, uri);
                    return new Bitmap(asset);
                }

                case ConsumableUI consumableUI:
                {
                    Logger.Debug("Processing consumableUI.");
                    string iconName = consumableUI.IconName;
                    Uri uri = new($"avares://{assemblyName}/Assets/consumable_icons/consumable_{iconName}.png");

                    Stream asset = LoadEmbeddedAsset(assets, uri);
                    return new Bitmap(asset);
                }

                case Exterior exterior:
                {
                    Logger.Debug("Processing Exterior.");
                    if (!string.IsNullOrEmpty(exterior.Name) && parameter != null && parameter.ToString()!.Equals("Flags", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Uri uri = new($"avares://{assemblyName}/Assets/signal_flags/{exterior.Name}.png");
                        Stream asset = LoadEmbeddedAsset(assets, uri);
                        return new Bitmap(asset);
                    }
                    else if (string.IsNullOrEmpty(exterior.Name))
                    {
                        Uri uri = new($"avares://{assemblyName}/Assets/blank.png");
                        Stream asset = LoadEmbeddedAsset(assets, uri);
                        return new Bitmap(asset);
                    }
                    else
                    {
                        Stream asset = LoadEmbeddedAsset(assets, null);
                        return new Bitmap(asset);
                    }
                }

                default:
                {
                    Logger.Warn($"Unable to find handler for type {value?.GetType()}. Falling back to default implementation.");
                    Stream asset = LoadEmbeddedAsset(assets, null);
                    return new Bitmap(asset);
                }
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Helper method to load an embedded asset as stream.
        /// </summary>
        /// <param name="assetLoader">The <see cref="IAssetLoader"/> used to access embedded assets.</param>
        /// <param name="uri">The <see cref="Uri"/> of the asset.</param>
        /// <returns>A stream of the asset or the error icon, if the asset did not exist.</returns>
        private static Stream LoadEmbeddedAsset(IAssetLoader assetLoader, Uri? uri)
        {
            if (uri == null)
            {
                return LoadErrorIcon(assetLoader);
            }

            Stream result;
            try
            {
                result = assetLoader.Open(uri);
            }
            catch (Exception e) when (e is NullReferenceException or FileNotFoundException)
            {
                Logger.Warn("Unable to find file for uri {0}, falling back to error icon.", uri);
                result = LoadErrorIcon(assetLoader);
            }

            return result;
        }

        private static Stream LoadErrorIcon(IAssetLoader assetLoader)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
            return assetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Icons/Error.png"));
        }

        private static string GetSkillimageId(string skillName)
        {
            if (skillName == null)
            {
                throw new ArgumentNullException(nameof(skillName));
            }

            if (skillName.Length < 2)
            {
                return skillName;
            }

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(skillName[0]));
            for (var i = 1; i < skillName.Length; ++i)
            {
                char c = skillName[i];
                if (char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
