using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Newtonsoft.Json;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataProvider
{
    public class AppDataHelper
    {
        #region Static Fields and Constants

        private static readonly Lazy<AppDataHelper> InstanceValue = new(() => new AppDataHelper());

        #endregion

        private readonly IFileSystem fileSystem;

        private AppDataHelper()
            : this(new FileSystem())
        {
        }

        internal AppDataHelper(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public static AppDataHelper Instance => InstanceValue.Value;

        public string AppDataDirectory => fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWsShipBuilder");

        public string AppDataImageDirectory => fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWsShipBuilder", "Images");

        public string GetDataPath(ServerType serverType)
        {
            string serverName = serverType == ServerType.Live ? "live" : "pts";
            return fileSystem.Path.Combine(AppDataDirectory, "json", serverName);
        }

        public Dictionary<string, T>? ReadLocalJsonData<T>(Nation? nation, ServerType serverType)
        {
            string categoryString = GetCategoryString<T>();
            string nationString = GetNationString(nation);
            string fileName = fileSystem.Path.Combine(GetDataPath(serverType), categoryString, $"{nationString}.json");
            return DeserializeFile<Dictionary<string, T>>(fileName);
        }

        public List<ShipSummary> GetShipSummaryList(ServerType serverType)
        {
            string fileName = fileSystem.Path.Combine(GetDataPath(serverType), "Summary", "Common.json");
            return DeserializeFile<List<ShipSummary>>(fileName) ?? new List<ShipSummary>();
        }

        public Dictionary<string, string>? ReadLocalizationData(ServerType serverType, string language)
        {
            string fileName = fileSystem.Path.Combine(GetDataPath(serverType), "Localization", $"{language}.json");
            return fileSystem.File.Exists(fileName) ? DeserializeFile<Dictionary<string, string>>(fileName) : null;
        }

        public Ship? GetShipFromSummary(ShipSummary summary, bool changeDictionary = true)
        {
            Ship? ship = null;

            if (summary.Nation.Equals(AppData.CurrentLoadedNation))
            {
                ship = AppData.ShipDictionary![summary.Index];
            }
            else
            {
                var shipDict = ReadLocalJsonData<Ship>(summary.Nation, AppData.Settings.SelectedServerType);
                if (shipDict != null)
                {
                    ship = shipDict[summary.Index];
                    if (changeDictionary)
                    {
                        AppData.ShipDictionary = shipDict;
                        AppData.CurrentLoadedNation = summary.Nation;
                    }
                }
            }

            return ship;
        }

        private static string GetNationString(Nation? nation)
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

        private static string GetCategoryString<T>()
        {
            return typeof(T) switch
            {
                var consumableType when consumableType == typeof(Consumable) => "Ability",
                var aircraftType when aircraftType == typeof(Aircraft) => "Aircraft",
                var crewType when crewType == typeof(Captain) => "Crew",
                var exteriorType when exteriorType == typeof(Exterior) => "Exterior",
                var gunType when gunType == typeof(Gun) => "Gun",
                var modernizationType when modernizationType == typeof(Modernization) => "Modernization",
                var projectileType when typeof(Projectile).IsAssignableFrom(projectileType) => "Projectile",
                var shipType when shipType == typeof(Ship) => "Ship",
                var moduleType when moduleType == typeof(Module) => "Unit",
                _ => throw new InvalidOperationException(),
            };
        }

        private T? DeserializeFile<T>(string filePath)
        {
            using Stream fs = fileSystem.File.OpenRead(filePath);
            var streamReader = new StreamReader(fs);
            var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<T>(jsonReader);
        }
    }
}
