using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataProvider
{
    // TODO: Integrate with versioning as soon as it is available.
    public static class AppDataHelper
    {
        public static string AppDataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWsShipBuilder");

        public static Dictionary<string, T>? ReadLocalJsonData<T>(Nation? nation)
        {
            string categoryString = GetCategoryString<T>();
            string nationString = GetNationString(nation);
            string fileName = Path.Combine(AppDataDirectory, "json", categoryString, $"{nationString}.json");
            using FileStream fs = File.OpenRead(fileName);
            var streamReader = new StreamReader(fs);
            var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<Dictionary<string, T>>(jsonReader);
        }

        public static string GetNationString(Nation? nation)
        {
            return nation switch
            {
                Nation.PanAmerica => "Pan_America",
                Nation.PanAsia => "Pan_Asia",
                Nation.UnitedKingdom => "United_Kingdom",
                Nation.Usa => "USA",
                null => "Common",
                _ => nation.ToString() ?? throw new InvalidOperationException("Unable to retrieve enum name."),
            };
        }

        public static string GetCategoryString<T>()
        {
            return typeof(T) switch
            {
                var consumableType when consumableType == typeof(Consumable) => "Ability",
                var aircraftType when aircraftType == typeof(Aircraft) => "Aircraft",
                var crewType when crewType == typeof(Captain) => "Crew",
                var exteriorType when exteriorType == typeof(Exterior) => "Exterior",
                var gunType when gunType == typeof(Gun) => "Gun",
                var modernizationType when modernizationType == typeof(Modernization) => "Modernization",
                var projectileType when projectileType == typeof(Projectile) => "Projectile",
                var shipType when shipType == typeof(Ship) => "Ship",
                var moduleType when moduleType == typeof(Module) => "Unit",
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
