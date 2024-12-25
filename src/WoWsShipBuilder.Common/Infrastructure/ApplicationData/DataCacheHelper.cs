using System.Collections.Immutable;
using System.Text.Json;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Exterior;
using WoWsShipBuilder.DataStructures.Module;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Infrastructure.ApplicationData;

public static class DataCacheHelper
{
    private static readonly SemaphoreSlim Semaphore = new(1);

    public static async Task AddToCache(string fileName, string category, string content)
    {
        var parseResult = Enum.TryParse(fileName.Replace(".json", string.Empty).Replace("_", string.Empty), true, out Nation nation);
        if (!parseResult)
        {
            throw new InvalidOperationException();
        }

        var type = category.ToLowerInvariant() switch
        {
            "ability" => typeof(Dictionary<string, Consumable>),
            "aircraft" => typeof(Dictionary<string, Aircraft>),
            "crew" => typeof(Dictionary<string, Captain>),
            "exterior" => typeof(Dictionary<string, Exterior>),
            "modernization" => typeof(Dictionary<string, Modernization>),
            "projectile" => typeof(Dictionary<string, Projectile>),
            "ship" => typeof(Dictionary<string, Ship>),
            "unit" => typeof(Dictionary<string, Module>),
            "summary" => typeof(List<ShipSummary>),
            _ => throw new InvalidOperationException(),
        };

        object? jsonObject = JsonSerializer.Deserialize(content, type, AppConstants.JsonSerializerOptions);

        await Semaphore.WaitAsync();
        switch (category.ToLowerInvariant())
        {
            case "ability":
                if (jsonObject is Dictionary<string, Consumable> consumables)
                {
                    AppData.ConsumableList = consumables;
                }

                break;
            case "aircraft":
                AppData.AircraftCache.SetIfNotNull(nation, (Dictionary<string, Aircraft>?)jsonObject);
                break;
            case "crew":
                AppData.CaptainCache.SetIfNotNull(nation, (Dictionary<string, Captain>?)jsonObject);
                break;
            case "exterior":
                AppData.ExteriorCache.SetIfNotNull(nation, (Dictionary<string, Exterior>?)jsonObject);
                break;
            case "modernization":
                if (jsonObject is Dictionary<string, Modernization> modernizations)
                {
                    AppData.ModernizationCache = modernizations;
                }

                break;
            case "projectile":
                AppData.ProjectileCache.SetIfNotNull(nation, (Dictionary<string, Projectile>?)jsonObject);
                break;
            case "ship":
                if (jsonObject is Dictionary<string, Ship> ships)
                {
                    foreach (var ship in ships)
                    {
                        AppData.ShipDictionary.Add(ship.Key, ship.Value);
                    }
                }

                break;
            case "unit":
                // TODO: add once unit is actually needed
                break;
            case "summary":
                if (jsonObject is List<ShipSummary> shipSummaries)
                {
                    AppData.ShipSummaryMapper = shipSummaries.ToImmutableDictionary(x => x.Index);
                }

                break;
        }

        Semaphore.Release();
    }
}
