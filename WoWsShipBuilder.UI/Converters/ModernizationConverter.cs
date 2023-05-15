using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;
using WoWsShipBuilder.Common.ShipStats.ViewModels;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.DataStructures.Upgrade;
using WoWsShipBuilder.ViewModels.ShipVm;

namespace WoWsShipBuilder.UI.Converters
{
    public class ModernizationConverter : IMultiValueConverter
    {
        private static readonly ImagePathConverter ImagePathConverter = new();

        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is List<Modernization> modernizations && values[1] is CustomObservableCollection<Modernization> selectedModernizations)
            {
                switch (parameter)
                {
                    case "index":
                        int index = modernizations.FindIndex(mod => selectedModernizations.Any(selected => selected.Index.Equals(mod.Index)));
                        return index > -1 ? index : 0;
                    case "image":
                    {
                        Modernization? selectedMod = modernizations.FirstOrDefault(modernization => selectedModernizations.Any(selected => selected.Index.Equals(modernization.Index)));
                        selectedMod ??= UpgradePanelViewModelBase.PlaceholderModernization;
                        return ImagePathConverter.Convert(selectedMod, targetType, null!, culture);
                    }

                    case "data":
                    {
                        Modernization? selectedMod = modernizations.FirstOrDefault(modernization => selectedModernizations.Any(selected => selected.Index.Equals(modernization.Index)));
                        selectedMod ??= UpgradePanelViewModelBase.PlaceholderModernization;
                        return selectedMod;
                    }
                }
            }

            return new BindingNotification(new NotSupportedException());
        }
    }
}
