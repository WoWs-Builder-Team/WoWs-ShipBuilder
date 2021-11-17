using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.BuildCreator;
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
            DefaultAppDataDirectory = fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WoWsShipBuilder");
        }

        public static AppDataHelper Instance => InstanceValue.Value;

        public string DefaultAppDataDirectory { get; }

        public string AppDataDirectory
        {
            get
            {
                if (AppData.IsInitialized)
                {
                    return AppData.Settings.CustomDataPath ?? DefaultAppDataDirectory;
                }

                return DefaultAppDataDirectory;
            }
        }

        public string AppDataImageDirectory => fileSystem.Path.Combine(AppDataDirectory, "Images");

        public string GetDataPath(ServerType serverType)
        {
            string serverName = serverType == ServerType.Live ? "live" : "pts";
            return fileSystem.Path.Combine(AppDataDirectory, "json", serverName);
        }

        /// <summary>
        /// Gets the localization directory for the selected server type.
        /// </summary>
        /// <param name="serverType">The selected server type.</param>
        /// <returns>The directory path of the current localization directory.</returns>
        public string GetLocalizationPath(ServerType serverType) => fileSystem.Path.Combine(GetDataPath(serverType), "Localization");

        /// <summary>
        /// Find the list of currently installed localizations.
        /// </summary>
        /// <param name="serverType">The selected server type.</param>
        /// <returns>A possibly empty list of installed locales.</returns>
        public List<string> GetInstalledLocales(ServerType serverType)
        {
            fileSystem.Directory.CreateDirectory(GetLocalizationPath(serverType));
            return fileSystem.Directory.GetFiles(GetLocalizationPath(serverType)).ToList();
        }

        /// <summary>
        /// Read a dictionary from the local app data directory.
        /// </summary>
        /// <param name="nation">The selected nation.</param>
        /// <param name="serverType">The selected server type.</param>
        /// <typeparam name="T">The data type of the values of the dictionary.</typeparam>
        /// <returns>A dictionary containing the deserialized file content.</returns>
        public Dictionary<string, T>? ReadLocalJsonData<T>(Nation nation, ServerType serverType)
        {
            string categoryString = GetCategoryString<T>();
            string nationString = GetNationString(nation);
            string fileName = fileSystem.Path.Combine(GetDataPath(serverType), categoryString, $"{nationString}.json");
            return DeserializeFile<Dictionary<string, T>>(fileName);
        }

        /// <summary>
        /// Read the current version info from the app data directory.
        /// </summary>
        /// <param name="serverType">The selected server type.</param>
        /// <returns>The local VersionInfo or null if none was found.</returns>
        public virtual VersionInfo? ReadLocalVersionInfo(ServerType serverType)
        {
            string filePath = fileSystem.Path.Combine(GetDataPath(serverType), "VersionInfo.json");
            return DeserializeFile<VersionInfo>(filePath);
        }

        public List<ShipSummary> GetShipSummaryList(ServerType serverType)
        {
            string fileName = fileSystem.Path.Combine(GetDataPath(serverType), "Summary", "Common.json");
            return DeserializeFile<List<ShipSummary>>(fileName) ?? new List<ShipSummary>();
        }

        public void LoadNationFiles(Nation nation)
        {
            var server = AppData.Settings.SelectedServerType;
            AppData.ProjectileList = ReadLocalJsonData<Projectile>(nation, server);
            AppData.AircraftList = ReadLocalJsonData<Aircraft>(nation, server);
            if (AppData.ConsumableList is null)
            {
                AppData.ConsumableList = ReadLocalJsonData<Consumable>(Nation.Common, server);
            }
        }

        public Aircraft FindAswAircraft(string planeIndex)
        {
            if (AppData.AircraftList!.ContainsKey(planeIndex))
            {
                return AppData.AircraftList[planeIndex];
            }

            Nation nation = planeIndex.ToUpperInvariant()[1] switch
            {
                'A' => Nation.Usa,
                'J' => Nation.Japan,
                'H' => Nation.Netherlands,
                'B' => Nation.UnitedKingdom,
                _ => throw new InvalidOperationException(),
            };

            return ReadLocalJsonData<Aircraft>(nation, AppData.Settings.SelectedServerType)![planeIndex];
        }

        public Projectile FindAswDepthCharge(string depthChargeName)
        {
            if (AppData.ProjectileList!.ContainsKey(depthChargeName))
            {
                return AppData.ProjectileList[depthChargeName];
            }

            Nation nation = depthChargeName.ToUpperInvariant()[1] switch
            {
                'A' => Nation.Usa,
                'J' => Nation.Japan,
                'H' => Nation.Netherlands,
                'B' => Nation.UnitedKingdom,
                _ => throw new InvalidOperationException(),
            };

            return ReadLocalJsonData<Projectile>(nation, AppData.Settings.SelectedServerType)![depthChargeName];
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

        /// <summary>
        /// Save string compressed <see cref="Build"/> to the disk.
        /// </summary>
        public void SaveBuilds()
        {
            var path = fileSystem.Path.Combine(Instance.DefaultAppDataDirectory, "builds.json");
            var builds = AppData.Builds.Select(build => build.CreateStringFromBuild()).ToList();
            string buildsString = JsonConvert.SerializeObject(builds);
            fileSystem.File.WriteAllText(path, buildsString);
        }

        public void LoadBuilds()
        {
            string path = fileSystem.Path.Combine(Instance.DefaultAppDataDirectory, "builds.json");
            if (fileSystem.File.Exists(path))
            {
                var rawBuildList = JsonConvert.DeserializeObject<List<string>>(fileSystem.File.ReadAllText(path));
                AppData.Builds = rawBuildList?.Select(Build.CreateBuildFromString).ToList() ?? new List<Build>();
            }
        }

        internal T? DeserializeFile<T>(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("The provided file path must not be empty.");
            }

            if (!fileSystem.File.Exists(filePath))
            {
                Logging.Logger.Warn($"Tried to load file {filePath}, but it was not found.");
                return default;
            }

            using Stream fs = fileSystem.File.OpenRead(filePath);
            var streamReader = new StreamReader(fs);
            var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<T>(jsonReader);
        }

        private static string GetNationString(Nation nation)
        {
            return nation switch
            {
                Nation.PanAmerica => "Pan_America",
                Nation.PanAsia => "Pan_Asia",
                Nation.UnitedKingdom => "United_Kingdom",
                Nation.Usa => "USA",
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
    }
}
