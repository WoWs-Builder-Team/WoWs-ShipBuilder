using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataContainers;

namespace WoWsShipBuilder.UI.Converters
{
    public class ConsumableUiConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2)
            {
                return "";
            }

            if (values[0] is List<ConsumableDataContainer> consumables && values[1] is CustomObservableCollection<ConsumableDataContainer> selectedConsumables)
            {
                switch (parameter)
                {
                    case "image":
                    {
                        ConsumableDataContainer? selectedConsumable = consumables.FirstOrDefault(consumable =>
                            selectedConsumables.Any(selected => selected.IconName.Equals(consumable.IconName)));
                        return new ImagePathConverter().Convert(selectedConsumable, typeof(Bitmap), null!, CultureInfo.CurrentCulture);
                    }

                    case "index":
                    {
                        return consumables.FindIndex(consumable => selectedConsumables.Any(selected => selected.IconName.Equals(consumable.IconName)));
                    }

                    case "data":
                    {
                        ConsumableDataContainer? selectedConsumable = consumables
                            .FirstOrDefault(consumable => selectedConsumables.Any(selected => selected.IconName.Equals(consumable.IconName)));
                        return selectedConsumable != null ? selectedConsumable : "";
                    }
                }
            }

            Logging.Logger.Trace("No matching processing path for consumable data conversion found. Element 1: {0}, Element 2: {1}", values[0], values[1]);
            return new BindingNotification(new NotSupportedException());
        }
    }
}
