using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.WebWorkers;

public static class DataServiceExtensions
{
    private static readonly Dictionary<string, Ship> ShipCache = new();

    public static async Task<Ship> LoadShipAsync(this IDataService dataService, string shipIndex, string dataPath)
    {
        if (!ShipCache.ContainsKey(shipIndex))
        {
            var nationShips = await dataService.LoadAsync<Dictionary<string, Ship>>(dataPath) ?? new();
            foreach ((string? key, var value) in nationShips)
            {
                ShipCache[key] = value;
            }
        }

        ShipCache.Remove(shipIndex, out var ship);
        return ship!;
    }
}
