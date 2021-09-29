using System.Collections.Generic;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI
{
    public static class DataHelper
    {
        public static readonly Modernization PlaceholderModernization = new() { Index = null };

        public static readonly IReadOnlyList<Modernization> PlaceholderBaseList = new List<Modernization> { PlaceholderModernization };
    }
}
