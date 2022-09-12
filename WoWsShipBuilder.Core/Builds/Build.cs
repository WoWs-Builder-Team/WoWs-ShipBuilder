using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Newtonsoft.Json;
using NLog;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.Builds;

public class Build
{
    internal const int CurrentBuildVersion = 2;

    [JsonConstructor]
    private Build(string buildName, string shipIndex, Nation nation, List<string> modules, List<string> upgrades, List<string> consumables, string captain, List<int> skills, List<string> signals, int buildVersion = CurrentBuildVersion)
    {
        BuildName = buildName;
        ShipIndex = shipIndex;
        Nation = nation;
        Modules = modules;
        Upgrades = upgrades;
        Captain = captain;
        Skills = skills;
        Signals = signals;
        Consumables = consumables;
        BuildVersion = buildVersion;

        Hash = CreateHash(this);
    }

    public Build(string buildName, string shipIndex, Nation nation, List<string> modules, List<string> upgrades, List<string> consumables, string captain, List<int> skills, List<string> signals)
        : this(buildName, shipIndex, nation, modules, upgrades, consumables, captain, skills, signals, CurrentBuildVersion)
    {
    }

    [JsonProperty(Required = Required.Always)]
    public string BuildName { get; }

    public string ShipIndex { get; }

    public Nation Nation { get; }

    public IReadOnlyList<string> Modules { get; }

    public IReadOnlyList<string> Upgrades { get; }

    public string Captain { get; }

    public IReadOnlyList<int> Skills { get; }

    public IReadOnlyList<string> Consumables { get; }

    public IReadOnlyList<string> Signals { get; }

    public int BuildVersion { get; private set; }

    [JsonIgnore]
    public string Hash { get; }

    /// <summary>
    /// Create a new <see cref="Build"/> from a compressed and base64 encoded string.
    /// </summary>
    /// <param name="buildString">The build string.</param>
    /// <param name="logger">An optional logger to use for logging results.</param>
    /// <returns>The <see cref="Build"/> object represented by the string.</returns>
    /// <exception cref="FormatException">If the string is not in the correct format.</exception>
    public static Build CreateBuildFromString(string buildString, ILogger? logger = null)
    {
        var effectiveLogger = logger ?? Logging.Logger;
        try
        {
            byte[] decodedOutput = Convert.FromBase64String(buildString);
            using var inputStream = new MemoryStream(decodedOutput);
            using var gzip = new DeflateStream(inputStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, System.Text.Encoding.UTF8);
            string buildJson = reader.ReadToEnd();
            var build = JsonConvert.DeserializeObject<Build>(buildJson) ?? throw new InvalidOperationException("Failed to deserialize build object from string");
            return UpgradeBuild(build, effectiveLogger);
        }
        catch (Exception e)
        {
            effectiveLogger.Warn(e, $"The string {buildString} is not in a valid format for a Build object");
            throw new FormatException("Invalid build string format", e);
        }
    }

    /// <summary>
    /// Helper method to upgrade a build from an old build format to the current build format.
    /// </summary>
    /// <param name="oldBuild">The old build.</param>
    /// <param name="logger">An optional logger to use for logging results.</param>
    /// <returns>The updated build.</returns>
    private static Build UpgradeBuild(Build oldBuild, ILogger logger)
    {
        if (oldBuild.BuildVersion < CurrentBuildVersion)
        {
            logger.Info("Upgrading build {} from build version {} to current version.", oldBuild.BuildName, oldBuild.BuildVersion);
            oldBuild.BuildVersion = CurrentBuildVersion;
        }

        return oldBuild;
    }

    private static string CreateHash(Build build)
    {
        string buildString = JsonConvert.SerializeObject(build);
        using var sha = SHA256.Create();
        byte[] textData = System.Text.Encoding.UTF8.GetBytes(buildString);
        byte[] hash = sha.ComputeHash(textData);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        hashCode.Add(Hash);
        return hashCode.ToHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is Build build)
        {
            return Hash == build.Hash;
        }

        return false;
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
