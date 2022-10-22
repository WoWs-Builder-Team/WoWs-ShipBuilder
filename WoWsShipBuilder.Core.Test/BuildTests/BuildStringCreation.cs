using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.Test.BuildTests;

[TestFixture]
public class BuildStringCreation
{
    private readonly Regex buildRegex = new(@"^(?<shipIndex>[^;]*);(?<modules>[A-Z0-9,]*);(?<upgrades>[A-Z0-9,]*);(?<captain>[A-Z0-9]+);(?<skills>[0-9,]*);(?<consumables>[A-Z0-9,]*);(?<signals>[A-Z0-9,]*);(?<version>\d+)(;(?<buildName>[^;]*))?$");

    [Test]
    public void EmptyBuild_CreateShortString_ExpectedResult()
    {
        const string buildName = "test-build";
        const string shipIndex = "PASC020";
        string expectedString = $"{shipIndex};;;PCW001;;;;{Build.CurrentBuildVersion};{buildName}";
        var build = new Build(buildName, shipIndex, Nation.Usa, new(), new(), new(), "PCW001", new(), new());

        string result = build.CreateShortStringFromBuild();

        result.Should().Be(expectedString);
    }

    [Test]
    public void EmptyBuild_CreateShortStringWithBuildName_MatchesRegex()
    {
        const string buildName = "test-build";
        const string shipIndex = "PASC020";
        var build = new Build(buildName, shipIndex, Nation.Usa, new(), new(), new(), "PCW001", new(), new());
        string buildString = build.CreateShortStringFromBuild();

        var result = buildRegex.Match(buildString);

        result.Success.Should().BeTrue();
        result.Length.Should().Be(buildString.Length);
        result.Groups["shipIndex"].Success.Should().BeTrue();
        result.Groups["shipIndex"].Value.Should().Be(shipIndex);
        result.Groups["buildName"].Success.Should().BeTrue();
        result.Groups["buildName"].Value.Should().Be(buildName);
    }

    [Test]
    public void EmptyBuild_CreateShortStringWithoutBuildName_MatchesRegex()
    {
        const string shipIndex = "PASC020";
        var build = new Build(string.Empty, shipIndex, Nation.Usa, new(), new(), new(), "PCW001", new(), new());
        string buildString = build.CreateShortStringFromBuild();

        var result = buildRegex.Match(buildString);

        result.Success.Should().BeTrue();
        result.Length.Should().Be(buildString.Length);
        result.Groups["shipIndex"].Success.Should().BeTrue();
        result.Groups["shipIndex"].Value.Should().Be(shipIndex);
        result.Groups["buildName"].Success.Should().BeTrue();
        result.Groups["buildName"].Value.Should().BeEmpty();
    }
}
