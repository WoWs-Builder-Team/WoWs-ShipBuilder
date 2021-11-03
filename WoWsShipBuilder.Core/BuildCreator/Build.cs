using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

namespace WoWsShipBuilder.Core.BuildCreator
{
    public class Build
    {
        public Build(string? shipName, List<string>? modules, List<string>? consumables, List<int>? skills, List<string>? signals)
        {
            ShipName = shipName;
            Modules = modules;
            Skills = skills;
            Signals = signals;
            Consumables = consumables;
        }

        public string? BuildName { get; set; }

        public string? ShipName { get; set; }

        public List<string>? Modules { get; set; }

        public List<int>? Skills { get; set; }

        public List<string>? Consumables { get; set; }

        public List<string>? Signals { get; set; }

        public static Build? CreateBuildFromString(string buildString)
        {
            var decodedOutput = System.Convert.FromBase64String(buildString);
            using (MemoryStream inputStream = new MemoryStream(decodedOutput))
            {
                using (DeflateStream gzip =
                  new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader =
                      new StreamReader(gzip, System.Text.Encoding.UTF8))
                    {
                        var buildJson = reader.ReadToEnd();
                        var build = JsonConvert.DeserializeObject<Build>(buildJson);
                        return build;
                    }
                }
            }
        }

        public string CreateStringFromBuild()
        {
            var buildString = JsonConvert.SerializeObject(this);
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream gzip =
                  new DeflateStream(output, CompressionLevel.Optimal))
                {
                    using (StreamWriter writer =
                      new StreamWriter(gzip, System.Text.Encoding.UTF8))
                    {
                        writer.Write(buildString);
                    }
                }

                var bytes = output.ToArray();
                var encodedOutput = System.Convert.ToBase64String(bytes);
                return encodedOutput;
            }
        }
    }
}
