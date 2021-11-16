using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.DataProvider;

namespace WoWsShipBuilder.Core.Extensions
{
    public static class Extensions
    {
        // ReSharper disable once InconsistentNaming
        public static void AddDict<T, S>(this Dictionary<T, S?> thisDict, Dictionary<T, S?> otherDict) where T : struct where S : class
        {
            foreach ((T key, S? value) in otherDict)
            {
                thisDict.Add(key, value);
            }
        }

        public static int FindModifierIndex(this List<(string Key, float Value)> dataSource, string filter, bool strict = false)
        {
            if (strict)
            {
                return dataSource.FindIndex(modifier => modifier.Key.Equals(filter, StringComparison.InvariantCultureIgnoreCase));
            }

            return dataSource.FindIndex(modifier => modifier.Key.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }

        public static IEnumerable<float> FindModifiers(this IEnumerable<(string Key, float Value)> dataSource, string filter, bool strict = false)
        {
            if (strict)
            {
                return dataSource.Where(modifier => modifier.Key.Equals(filter, StringComparison.InvariantCultureIgnoreCase))
                            .Select(modifier => modifier.Value);
            }

            return dataSource.Where(modifier => modifier.Key.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            .Select(modifier => modifier.Value);
        }

        public static bool IsValidIndex(this int index) => index > -1;

        /// <summary>
        /// Converts a <see cref="ServerType"/> value into a string that can be used in urls or file system paths.
        /// </summary>
        /// <param name="serverType">The <see cref="ServerType"/> to convert.</param>
        /// <returns>A string representation of the server type.</returns>
        public static string StringName(this ServerType serverType)
        {
            return serverType == ServerType.Live ? "live" : "pts";
        }
    }
}
