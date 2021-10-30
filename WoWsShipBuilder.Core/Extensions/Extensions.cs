using System;
using System.Collections.Generic;
using System.Linq;

namespace WoWsShipBuilder.Core.Extensions
{
    public static class Extensions
    {
        public static void AddDict<T, S>(this Dictionary<T, S?> thisDict, Dictionary<T, S?> otherDict) where T : struct where S : class
        {
            foreach ((T key, S? value) in otherDict)
            {
                thisDict.Add(key, value);
            }
        }

        public static int FindModifierIndex(this List<(string Key, float Value)> dataSource, string filter)
        {
            return dataSource.FindIndex(modifier => modifier.Key.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<float> FindModifiers(this IEnumerable<(string Key, float Value)> dataSource, string filter) => dataSource
            .Where(modifier => modifier.Key.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            .Select(modifier => modifier.Value);

        public static bool IsValidIndex(this int index) => index > -1;
    }
}
