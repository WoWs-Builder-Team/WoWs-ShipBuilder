using System;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Updater;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    class ImagePathConverter : IValueConverter
    {
        private IFileSystem fileSystem;

        public ImagePathConverter()
            : this(new FileSystem())
        {
        }

        internal ImagePathConverter(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imageName && parameter is string type)
            {
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
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
                    var imagePath = fileSystem.Path.Combine(AppDataHelper.Instance.AppDataImageDirectory, "Ships", $"{imageName}.png");
                    if (!fileSystem.File.Exists(imagePath))
                    {
                        imagePath = fileSystem.Path.Combine(AppDataHelper.Instance.AppDataImageDirectory, "Ships", $"_default.png");
                    }

                    var stream = fileSystem.File.OpenRead(imagePath);
                    return new Bitmap(stream);
                }

                Stream? asset = assets.Open(uri);
                return new Bitmap(asset);
            }
            else if (value is Modernization modernization)
            {
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
            else
            {
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                Uri uri = new Uri($"avares://{assemblyName}/Assets/Icons/Error.png");
                Stream? asset = assets.Open(uri);
                return new Bitmap(asset);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
            for (int i = 1; i < skillName.Length; ++i)
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
