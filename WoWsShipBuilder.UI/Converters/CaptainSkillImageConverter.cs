using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Converters
{
    public class CaptainSkillImageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string skillName || !targetType.IsAssignableFrom(typeof(Bitmap)))
            {
                throw new NotSupportedException();
            }

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            Uri uri;
            if (skillName != null)
            {
                skillName = GetSkillimageId(skillName).ToLower();
                uri = new Uri($"avares://{assemblyName}/Assets/Skills/{skillName}.png");
            }
            else
            {
                uri = new Uri($"avares://{assemblyName}/Assets/Skills/missing.png");
            }

            Stream? asset = assets.Open(uri);
            return new Bitmap(asset);
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
