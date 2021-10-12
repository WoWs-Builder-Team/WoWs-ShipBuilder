using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imageName && parameter is string type)
            {
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                Uri uri = null;

                if (imageName.Equals("blank"))
                {
                    uri = new Uri($"avares://{assemblyName}/Assets/blank.png");
                }
                else if (type.Equals("skill", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (imageName != null)
                    {
                        imageName = GetSkillimageId(imageName).ToLower();
                        uri = new Uri($"avares://{assemblyName}/Assets/Skills/{imageName}.png");
                    }
                    else
                    {
                        uri = new Uri($"avares://{assemblyName}/Assets/Skills/missing.png");
                    }
                }
                else if (type.Equals("ship", StringComparison.InvariantCultureIgnoreCase))
                {
                    var imagePath = Path.Combine(AppDataHelper.Instance.AppDataImageDirectory, "Ships", $"{imageName}.png");
                    if (!File.Exists(imagePath))
                    {
                        imagePath = Path.Combine(AppDataHelper.Instance.AppDataImageDirectory, "Ships", $"_default.png");
                    }

                    var stream = File.OpenRead(imagePath);
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
                throw new NotSupportedException();
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
