using System.Collections.Generic;

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
    }
}
