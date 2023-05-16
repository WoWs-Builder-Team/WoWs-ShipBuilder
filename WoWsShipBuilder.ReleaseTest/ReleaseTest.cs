using System;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using WoWsShipBuilder.Desktop;
using WoWsShipBuilder.Desktop.ViewModels.Helper;
using WoWsShipBuilder.UI;

namespace WoWsShipBuilder.ReleaseTest;

public class Tests
{
    [Test]
    public void EnsureReleaseVersion()
    {
        var assembly = Assembly.GetAssembly(typeof(DesignDataHelper));
        string versionString = assembly!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        versionString = VersionHelper.StripCommitFromVersion(versionString);
        Version.TryParse(versionString, out _).Should().BeTrue();
    }

    [Test]
    public async Task EnsureNewVersionIsIncrement()
    {
        var assembly = Assembly.GetAssembly(typeof(DesignDataHelper));
        string versionString = assembly!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        versionString = VersionHelper.StripCommitFromVersion(versionString);
        var version = Version.Parse(versionString);

        const string uri = "https://api.github.com/repos/WoWs-Builder-Team/WoWs-ShipBuilder/releases/latest";
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "C# test client");
        var response = await client.GetStringAsync(uri);
        var apiResponse = JsonConvert.DeserializeObject<GithubApiResponse>(response);
        var lastVersionString = Regex.Replace(apiResponse!.Name, "[^0-9.]", string.Empty);
        var lastVersion = Version.Parse(lastVersionString);
        (lastVersion < version).Should().BeTrue();
    }
}
