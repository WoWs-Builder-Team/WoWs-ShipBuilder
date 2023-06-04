using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.GameData;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.Builds;

public class Build
{
    public const string DefaultBuildName = "---";

    internal const int CurrentBuildVersion = 4;

    private const char ListSeparator = ',';

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

    public IReadOnlyList<string> Modules { get; private set; }

    public IReadOnlyList<string> Upgrades { get; }

    public string Captain { get; }

    public IReadOnlyList<int> Skills { get; }

    public IReadOnlyList<string> Consumables { get; }

    public IReadOnlyList<string> Signals { get; private set; }

    /// <summary>
    /// Gets a value indicating the version number of a build.
    /// <br/><br/>
    /// <b>Version Changelog:</b>
    /// <para>v1: old build format that stored localized build data.</para>
    /// <para>v2: no longer store localized build data, store ship index directly.</para>
    /// <para>v3: no structural changes, signals are now stored by index instead of full name.</para>
    /// <para>v4: no structural changes, modules are now always stored by index instead of full name.</para>
    /// </summary>
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
            Build build;
            var byteOutput = new Span<byte>(new byte[buildString.Length]);
            if (Convert.TryFromBase64String(buildString, byteOutput, out int _))
            {
                byte[] decodedOutput = Convert.FromBase64String(buildString);
                using var inputStream = new MemoryStream(decodedOutput);
                using var gzip = new DeflateStream(inputStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzip, System.Text.Encoding.UTF8);
                string buildJson = reader.ReadToEnd();
                build = JsonConvert.DeserializeObject<Build>(buildJson) ?? throw new InvalidOperationException("Failed to deserialize build object from string");
            }
            else
            {
                build = CreateFromShortString(buildString);
            }

            return UpgradeBuild(build, effectiveLogger);
        }
        catch (Exception e)
        {
            effectiveLogger.LogWarning(e, "The string {BuildString} is not in a valid format for a Build object", buildString);
            throw new FormatException("Invalid build string format", e);
        }
    }

    internal static Build CreateFromShortString(string shortBuildString)
    {
        string[] parts = shortBuildString.Split(";");
        if (parts.Length < 8)
        {
            throw new InvalidOperationException("Received an invalid short build string");
        }

        string buildName = parts.Length == 9 ? parts[8] : string.Empty;
        string shipIndex = parts[0];
        var nation = GameDataHelper.GetNationFromIndex(shipIndex);
        List<string> modules = parts[1].Split(ListSeparator).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        List<string> upgrades = parts[2].Split(ListSeparator).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        string captain = parts[3];
        List<int> skills = parts[4].Split(ListSeparator).Where(x => !string.IsNullOrWhiteSpace(x)).Select(int.Parse).ToList();
        List<string> consumables = parts[5].Split(ListSeparator).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        List<string> signals = parts[6].Split(ListSeparator).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        int buildVersion = int.Parse(parts[7]);
        return new(buildName, shipIndex, nation, modules, upgrades, consumables, captain, skills, signals, buildVersion);
    }

    /// <summary>
    /// Helper method to upgrade a build from an old build format to the current build format.
    /// </summary>
    /// <param name="oldBuild">The old build.</param>
    /// <param name="logger">An optional logger to use for logging results.</param>
    /// <returns>The updated build.</returns>
    private static Build UpgradeBuild(Build oldBuild, ILogger logger)
    {
        if (oldBuild.BuildVersion < 3)
        {
            oldBuild.Signals = ReduceToIndex(oldBuild.Signals).ToList();
            logger.LogDebug("Reducing signal names to index for build {}", oldBuild.Hash);
        }

        if (oldBuild.BuildVersion < 4)
        {
            oldBuild.Modules = ReduceToIndex(oldBuild.Modules).ToList();
            logger.LogDebug("Reducing module names to index for build {}", oldBuild.Hash);
        }

        if (oldBuild.BuildVersion < CurrentBuildVersion)
        {
            logger.LogInformation("Upgrading build {OldBuildName}({OldBuildHash}) from build version {OldBuildVersion} to current version", oldBuild.BuildName, oldBuild.Hash, oldBuild.BuildVersion);
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

    public string CreateShortStringFromBuild()
    {
        string buildString = $"{ShipIndex};{string.Join(ListSeparator, ReduceToIndex(Modules))};{string.Join(ListSeparator, ReduceToIndex(Upgrades))};{Captain};{string.Join(ListSeparator, Skills)};{string.Join(ListSeparator, ReduceToIndex(Consumables))};{string.Join(ListSeparator, ReduceToIndex(Signals))};{BuildVersion};{BuildName}";
        return buildString;
    }

    private static IEnumerable<string> ReduceToIndex(IEnumerable<string> names)
    {
        return names.Select(x => x.Split("_").First());
    }
}
