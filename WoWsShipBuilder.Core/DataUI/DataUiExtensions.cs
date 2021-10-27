using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WoWsShipBuilder.Core.DataUI
{
    public static class DataUiExtensions
    {
        public static List<KeyValuePair<string, string>> ToPropertyMapping(this IDataUi dataUi, Func<(string Key, object? Value, DataUiUnitAttribute? Unit), bool>? filter = null)
        {
            filter ??= DefaultDataUiFilter;
            return dataUi.GetType().GetProperties()
                .Where(property => !property.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Any())
                .Select(property => (Key: "ShipStats_" + property.Name, Value: property.GetValue(dataUi),
                    Unit: (DataUiUnitAttribute?)property.GetCustomAttributes(typeof(DataUiUnitAttribute), false).FirstOrDefault()))
                .Where(filter)
                .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value + pair.Unit?.Localization))
                .ToList();
        }

        private static bool DefaultDataUiFilter((string Key, object? Value, DataUiUnitAttribute? Unit) pair)
        {
            return pair.Value switch
            {
                string strValue => !string.IsNullOrEmpty(strValue),
                decimal decValue => decValue != 0,
                (decimal min, decimal max) => min > 0 || max > 0,
                _ => false,
            };
        }
    }
}
