using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataUI;

namespace WoWsShipBuilder.UI.Converters
{
    public class ConsumableUiConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count < 2)
            {
                return "";
            }

            if (values[0] is List<ConsumableUI> consumables && values[1] is AvaloniaList<ConsumableUI> selectedConsumables)
            {
                switch (parameter)
                {
                    case "image":
                    {
                        ConsumableUI? selectedConsumable = consumables.FirstOrDefault(consumable =>
                            selectedConsumables.Any(selected => selected.IconName.Equals(consumable.IconName)));
                        return new ImagePathConverter().Convert(selectedConsumable, typeof(Bitmap), null!, CultureInfo.CurrentCulture);
                    }

                    case "index":
                    {
                        return consumables.FindIndex(consumable => selectedConsumables.Any(selected => selected.IconName.Equals(consumable.IconName)));
                    }

                    case "data":
                    {
                        ConsumableUI? selectedConsumable = consumables
                            .FirstOrDefault(consumable => selectedConsumables.Any(selected => selected.IconName.Equals(consumable.IconName)));
                        return selectedConsumable != null ? selectedConsumable : "";
                    }
                }
            }

            if (values[0] == AvaloniaProperty.UnsetValue || values[1] == AvaloniaProperty.UnsetValue)
            {
                Logging.Logger.Trace(new NotSupportedException());
                return new BindingNotification(AvaloniaProperty.UnsetValue);
            }

            return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
        }
    }
}
