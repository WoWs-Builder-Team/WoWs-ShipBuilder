using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Infrastructure.Utility;

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

    // ReSharper disable once InconsistentNaming
    public static void AddRange<T, S>(this Dictionary<T, S> thisDict, IEnumerable<KeyValuePair<T, S>> otherDict) where T : struct where S : class
    {
        foreach ((T key, S value) in otherDict)
        {
            thisDict.Add(key, value);
        }
    }

    // ReSharper disable once InconsistentNaming
    public static void RemoveMany<T, S>(this Dictionary<T, S> thisDict, IEnumerable<KeyValuePair<T, S>> otherDict) where T : struct where S : class => thisDict.RemoveMany(otherDict.Select(x => x.Key));

    // ReSharper disable once InconsistentNaming
    public static void RemoveMany<T, S>(this Dictionary<T, S> thisDict, IEnumerable<T> keyList) where T : struct where S : class
    {
        foreach (T key in keyList)
        {
            thisDict.Remove(key);
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
    /// Sets the value of the dictionary for the specified nation key if the value is not null.
    /// </summary>
    /// <param name="thisDict">The dictionary extended by this method.</param>
    /// <param name="nation">The <see cref="Nation"/> used as key.</param>
    /// <param name="content">The content dictionary for the key.</param>
    /// <typeparam name="T">The data type of the content dictionary.</typeparam>
    /// <returns><see langword="true"/> if the content was added, <see langword="false"/> otherwise.</returns>
    public static bool SetIfNotNull<T>(this IDictionary<Nation, Dictionary<string, T>> thisDict, Nation nation, Dictionary<string, T>? content)
    {
        if (content == null)
        {
            return false;
        }

        thisDict[nation] = content;
        return true;
    }

    public static string NameToIndex(this string name) => name.Split('_').First();
}
