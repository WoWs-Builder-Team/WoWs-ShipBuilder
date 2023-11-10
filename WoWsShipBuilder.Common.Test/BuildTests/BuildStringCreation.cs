using System.Collections.Immutable;
using System.Text.RegularExpressions;
using FluentAssertions;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Features.Builds;

namespace WoWsShipBuilder.Test.BuildTests;

[TestFixture]
public partial class BuildStringCreation
{
    private readonly Regex buildRegex = BuildRegex();

    [Test]
    public void EmptyBuild_CreateShortString_ExpectedResult()
    {
        const string buildName = "test-build";
        const string shipIndex = "PASC020";
        var expectedString = $"{shipIndex};;;PCW001;;;;{Build.CurrentBuildVersion};{buildName}";
        var build = new Build(buildName, shipIndex, Nation.Usa, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty, "PCW001", ImmutableArray<int>.Empty, ImmutableArray<string>.Empty);

        var result = build.CreateShortStringFromBuild();

        result.Should().Be(expectedString);
    }

    [Test]
    public void EmptyBuild_CreateShortStringWithBuildName_MatchesRegex()
    {
        const string buildName = "test-build";
        const string shipIndex = "PASC020";
        var build = new Build(buildName, shipIndex, Nation.Usa, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty, "PCW001", ImmutableArray<int>.Empty, ImmutableArray<string>.Empty);
        var buildString = build.CreateShortStringFromBuild();

        var result = this.buildRegex.Match(buildString);

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
        var build = new Build(string.Empty, shipIndex, Nation.Usa, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty, "PCW001", ImmutableArray<int>.Empty, ImmutableArray<string>.Empty);
        var buildString = build.CreateShortStringFromBuild();

        var result = this.buildRegex.Match(buildString);

        result.Success.Should().BeTrue();
        result.Length.Should().Be(buildString.Length);
        result.Groups["shipIndex"].Success.Should().BeTrue();
        result.Groups["shipIndex"].Value.Should().Be(shipIndex);
        result.Groups["buildName"].Success.Should().BeTrue();
        result.Groups["buildName"].Value.Should().BeEmpty();
    }

    [GeneratedRegex("^(?<shipIndex>[^;]*);(?<modules>[A-Z0-9,]*);(?<upgrades>[A-Z0-9,]*);(?<captain>[A-Z0-9]+);(?<skills>[0-9,]*);(?<consumables>[A-Z0-9,]*);(?<signals>[A-Z0-9,]*);(?<version>\\d+)(;(?<buildName>[^;]*))?$")]
    private static partial Regex BuildRegex();
}
