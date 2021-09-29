using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

namespace WoWsShipBuilder.Core.BuildCreator
{
    public static class BuildCreator
    {
        public static string CreateStringFromBuild(Build build)
        {
            var buildString = JsonConvert.SerializeObject(build);
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
    }
}
