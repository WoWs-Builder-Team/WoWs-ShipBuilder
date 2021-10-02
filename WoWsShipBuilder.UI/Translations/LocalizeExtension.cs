using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.UI.Translations
{
    public class LocalizeExtension : MarkupExtension
    {
        public LocalizeExtension(string key)
        {
            Key = key;
        }

        public string Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new ReflectionBindingExtension($"[{Key}]")
            {
                Mode = BindingMode.OneWay,
                Source = Localizer.Instance,
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
