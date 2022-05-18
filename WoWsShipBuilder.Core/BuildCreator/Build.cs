using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.BuildCreator
{
    public class Build
    {
        private const int CurrentBuildVersion = 2;

        [JsonConstructor]
        public Build(string buildName)
        {
            BuildName = buildName;
        }

        public Build(string shipIndex, Nation nation, List<string> modules, List<string> upgrades, List<string> consumables, string captain, List<int> skills, List<string> signals)
            : this(string.Empty)
        {
            ShipIndex = shipIndex;
            Nation = nation;
            Modules = modules;
            Upgrades = upgrades;
            Captain = captain;
            Skills = skills;
            Signals = signals;
            Consumables = consumables;
            BuildVersion = CurrentBuildVersion;
        }

        public string BuildName { get; set; }

        public string ShipIndex { get; set; } = string.Empty;

        public Nation Nation { get; set; }

        public List<string> Modules { get; set; } = new();

        public List<string> Upgrades { get; set; } = new();

        public string Captain { get; set; } = default!;

        public List<int> Skills { get; set; } = new();

        public List<string> Consumables { get; set; } = new();

        public List<string> Signals { get; set; } = new();

        public int BuildVersion { get; init; } = 1;

        /// <summary>
        /// Create a new <see cref="Build"/> from a compressed and base64 encoded string.
        /// </summary>
        /// <param name="buildString">The build string.</param>
        /// <param name="localizer">The <see cref="ILocalizer"/> used to resolve build localizations.</param>
        /// <returns>The <see cref="Build"/> object represented by the string.</returns>
        /// <exception cref="FormatException">If the string is not in the correct format.</exception>
        public static Build CreateBuildFromString(string buildString, ILocalizer localizer)
        {
            try
            {
                byte[] decodedOutput = Convert.FromBase64String(buildString);
                using var inputStream = new MemoryStream(decodedOutput);
                using var gzip = new DeflateStream(inputStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzip, System.Text.Encoding.UTF8);
                string buildJson = reader.ReadToEnd();
                var build = JsonConvert.DeserializeObject<Build>(buildJson);
                return UpgradeBuild(build!, localizer);
            }
            catch (Exception e)
            {
                Logging.Logger.Warn(e, $"The string {buildString} is not in a valid format for a Build object");
                throw new FormatException();
            }
        }

        public string CreateStringFromBuild()
        {
            string buildString = JsonConvert.SerializeObject(this);
            using var output = new MemoryStream();
            using (var gzip = new DeflateStream(output, CompressionLevel.Optimal))
            {
                using (var writer = new StreamWriter(gzip, System.Text.Encoding.UTF8))
                {
                    writer.Write(buildString);
                }
            }

            byte[] bytes = output.ToArray();
            string encodedOutput = Convert.ToBase64String(bytes);
            return encodedOutput;
        }

        /// <summary>
        /// Helper method to upgrade a build from an old build format to the current build format.
        /// </summary>
        /// <param name="oldBuild">The old build.</param>
        /// <param name="localizer">The localizer to use to resolve the ship name.</param>
        /// <returns>The updated build.</returns>
        private static Build UpgradeBuild(Build oldBuild, ILocalizer localizer)
        {
            if (oldBuild.BuildVersion <= 1)
            {
                Logging.Logger.Info("Upgrading build {} from build version {} to current version.", oldBuild.BuildName, oldBuild.BuildVersion);
                var buildShipName = " - " + localizer.GetGameLocalization(oldBuild.ShipIndex).Localization;
                if (oldBuild.BuildName.Contains(buildShipName))
                {
                    oldBuild.BuildName = oldBuild.BuildName.Replace(buildShipName, string.Empty);
                }
            }

            return oldBuild;
        }
    }
}
