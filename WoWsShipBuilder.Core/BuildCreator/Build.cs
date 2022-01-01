using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.BuildCreator
{
    public class Build
    {
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
        }

        public string BuildName { get; set; }

        public string ShipIndex { get; set; } = default!;

        public Nation Nation { get; set; }

        public List<string> Modules { get; set; } = new();

        public List<string> Upgrades { get; set; } = new();

        public string Captain { get; set; } = default!;

        public List<int> Skills { get; set; } = new();

        public List<string> Consumables { get; set; } = new();

        public List<string> Signals { get; set; } = new();

        /// <summary>
        /// Create a new <see cref="Build"/> from a compressed and base64 encoded string.
        /// </summary>
        /// <param name="buildString">The build string.</param>
        /// <returns>The <see cref="Build"/> object rapresented by the string.</returns>
        /// <exception cref="FormatException">If the string is not in the correct format.</exception>
        public static Build CreateBuildFromString(string buildString)
        {
            try
            {
                byte[] decodedOutput = Convert.FromBase64String(buildString);
                using var inputStream = new MemoryStream(decodedOutput);
                using var gzip = new DeflateStream(inputStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzip, System.Text.Encoding.UTF8);
                string buildJson = reader.ReadToEnd();
                var build = JsonConvert.DeserializeObject<Build>(buildJson);
                return build!;
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
    }
}
