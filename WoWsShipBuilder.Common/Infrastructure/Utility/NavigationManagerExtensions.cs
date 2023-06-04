using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace WoWsShipBuilder.Infrastructure.Utility;

public static class NavigationManagerExtensions
{
    /// <summary>
    /// Gets the value of the specified key from the URL query parameter.
    /// </summary>
    /// <param name="navManager">The <see cref="NavigationManager"/> instance.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">The variable where to store the value of the specified key.</param>
    /// <typeparam name="T"><see cref="string"/>, <see cref="int"/>, <see cref="decimal"/>.</typeparam>
    /// <returns>Returns if succeeded.</returns>
    public static bool TryGetQueryString<T>(this NavigationManager navManager, string key, out T value)
    {
        var uri = navManager.ToAbsoluteUri(navManager.Uri);

        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var valueFromQueryString))
        {
            if (typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, out var valueAsInt))
            {
                value = (T)(object)valueAsInt;
                return true;
            }

            if (typeof(T) == typeof(string))
            {
                value = (T)(object)valueFromQueryString.ToString();
                return true;
            }

            if (typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, out var valueAsDecimal))
            {
                value = (T)(object)valueAsDecimal;
                return true;
            }
        }

        value = default!;
        return false;
    }
}
