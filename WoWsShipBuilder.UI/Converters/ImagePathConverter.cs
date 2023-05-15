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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Splat;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Exterior;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;
using WoWsShipBuilder.UI.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WoWsShipBuilder.UI.Converters
{
    /// <summary>
    /// Multi-purpose converter to convert various types to an image.
    /// </summary>
    class ImagePathConverter : IValueConverter
    {
        // TODO: initialize properly
        private readonly ILogger logger;

        private readonly IFileSystem fileSystem;

        public ImagePathConverter()
            : this(Locator.Current.GetService<IFileSystem>() ?? new FileSystem(), Locator.Current.GetService<ILogger<ImagePathConverter>>() ?? NullLogger<ImagePathConverter>.Instance)
        {
        }

        internal ImagePathConverter(IFileSystem fileSystem, ILogger<ImagePathConverter> logger)
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
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
                    logger.LogDebug("Processing regular image name");
                    Uri? uri = null;

                    if (imageName.Equals("blank"))
                    {
                        uri = new($"avares://{assemblyName}/Assets/blank.png");
                    }
                    else if (type.Equals("skill", StringComparison.InvariantCultureIgnoreCase))
                    {
                        imageName = GetSkillImageId(imageName).ToLower();
                        uri = new($"avares://{assemblyName}/Assets/Skills/{imageName}.png");
                    }
                    else if (type.Equals("ship", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // TODO: remove locator invocation
                        string imagePath = fileSystem.Path.Combine(Locator.Current.GetRequiredService<IAppDataService>().AppDataImageDirectory, "Ships", $"{imageName}.png");
                        Stream stream;
                        if (fileSystem.File.Exists(imagePath))
                        {
                            stream = fileSystem.File.OpenRead(imagePath);
                        }
                        else
                        {
                            logger.LogWarning("Using fallback error icon because image {ImagePath} was not found", imagePath);
                            stream = LoadErrorIcon(assets);
                        }

                        return new Bitmap(stream);
                    }

                    return new Bitmap(LoadEmbeddedAsset(assets, uri, logger));
                }

                case Ship or ShipSummary:
                {
                    logger.LogDebug("Processing nation flag");
                    string shipIndex = default!;
                    string shipNation = default!;

                    switch (value)
                    {
                        case ShipSummary summary:
                            shipIndex = summary.Index;
                            shipNation = summary.Nation.ToString();
                            break;
                        case Ship ship:
                            shipIndex = ship.Index;
                            shipNation = ship.ShipNation.ToString();
                            break;
                    }

                    Uri uri = new($"avares://{assemblyName}/Assets/nation_flags/flag_{shipIndex}.png");

                    if (!assets.Exists(uri))
                    {
                        uri = new($"avares://{assemblyName}/Assets/nation_flags/flag_{shipNation}.png");
                    }

                    return new Bitmap(LoadEmbeddedAsset(assets, uri, logger));
                }

                case Modernization modernization:
                {
                    logger.LogDebug("Processing modernization");
                    Uri uri;
                    if (modernization.Index != null)
                    {
                        uri = new($"avares://{assemblyName}/Assets/modernization_icons/icon_modernization_{modernization.Name}.png");
                    }
                    else
                    {
                        uri = new($"avares://{assemblyName}/Assets/modernization_icons/modernization_default3.png");
                    }

                    return new Bitmap(LoadEmbeddedAsset(assets, uri, logger));
                }

                case Consumable or (ShipConsumable, Consumable):
                {
                    logger.LogDebug("Processing consumable");
                    if (value is not Consumable consumable)
                    {
                        consumable = (((ShipConsumable, Consumable tmpConsumable))value).tmpConsumable;
                    }

                    string iconName = string.IsNullOrEmpty(consumable.IconId) ? consumable.Name : consumable.IconId;
                    Uri uri = new($"avares://{assemblyName}/Assets/consumable_icons/consumable_{iconName}.png");

                    return new Bitmap(LoadEmbeddedAsset(assets, uri, logger));
                }

                case ConsumableDataContainer consumableUi:
                {
                    logger.LogDebug("Processing consumableUI");
                    string iconName = consumableUi.IconName;
                    Uri uri = new($"avares://{assemblyName}/Assets/consumable_icons/consumable_{iconName}.png");

                    return new Bitmap(LoadEmbeddedAsset(assets, uri, logger));
                }

                case Exterior exterior:
                {
                    logger.LogDebug("Processing Exterior");

                    Uri? uri = null;
                    if (!string.IsNullOrEmpty(exterior.Name) && parameter != null && parameter.ToString()!.Equals("Flags", StringComparison.InvariantCultureIgnoreCase))
                    {
                        uri = new($"avares://{assemblyName}/Assets/signal_flags/{exterior.Name}.png");
                    }
                    else if (string.IsNullOrEmpty(exterior.Name))
                    {
                        uri = new($"avares://{assemblyName}/Assets/blank.png");
                    }

                    return new Bitmap(LoadEmbeddedAsset(assets, uri, logger));
                }

                default:
                {
                    logger.LogWarning("Unable to find handler for type {Type}. Falling back to default implementation", value?.GetType());
                    return new Bitmap(LoadEmbeddedAsset(assets, null, logger));
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
        /// <param name="logger">The logger to use.</param>
        /// <returns>A stream of the asset or the error icon, if the asset did not exist.</returns>
        private static Stream LoadEmbeddedAsset(IAssetLoader assetLoader, Uri? uri, ILogger logger)
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
                logger.LogWarning("Unable to find file for uri {Uri}, falling back to error icon", uri);
                result = LoadErrorIcon(assetLoader);
            }

            return result;
        }

        private static Stream LoadErrorIcon(IAssetLoader assetLoader)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
            return assetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Icons/Error.png"));
        }

        private static string GetSkillImageId(string skillName)
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
