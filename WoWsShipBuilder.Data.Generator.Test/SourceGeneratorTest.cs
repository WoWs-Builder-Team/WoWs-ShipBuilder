using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using WoWsShipBuilder.Data.Generator.Attributes;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace WoWsShipBuilder.Data.Generator.Test;

[TestFixture]
public class SourceGeneratorTest
{
    private GeneratorDriver rawDriver = default!;

    private string baseCompilationInput = default!;

    [OneTimeSetUp]
    public void FixtureSetup()
    {
        baseCompilationInput = File.ReadAllText(Path.Combine("TestStructures", "DataElements.cs"));
    }

    [SetUp]
    public void Setup()
    {
        rawDriver = CSharpGeneratorDriver.Create(new DataElementSourceGenerator());
    }

    [Test]
    public void NoImplementations_NoErrors()
    {
        var compilation = CreateCompilation(baseCompilationInput);
        var driver = rawDriver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();
    }

    [Test]
    public void AllTest()
    {
        var code = @"using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;

namespace WoWsShipBuilder.Data.Generator.Test.TestStructures;

public partial record TestDataUi1 : ProjectileDataContainer
{
    [DataElementType(DataElementTypes.Value)]
    public string TestValue { get; init; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    [DataElementVisibility(false)]
    public decimal TestKeyValue { get; init; }

    [DataElementType(DataElementTypes.KeyValue)]
    [DataElementVisibility(true, ""TestVisibility"")]
    public decimal TestVisibilityCustom { get; init; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = ""mm"")]
    public string TestKeyUnitValue { get; init; } = default!;

    [DataElementType(DataElementTypes.Tooltip, TooltipKey = ""testTooltip"")]
    public decimal TestTooltipValue { get; init; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = ""test1"")]
    public string TestGroup1 { get; init; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = ""test1"")]
    public string Test2Group1 { get; init; } = default!;

    public bool TestVisibility(object value)
    {
        return true;
    }
}
";
        var compilation = CreateCompilation(baseCompilationInput, code);
        var driver = rawDriver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

         //diagnostics.Should().NotBeEmpty();
        //diagnostics.Should().BeEmpty();
    }

    [Test]
    public void ManeuverabilityDataContainer_NoErrors()
    {
        var code = @"using WoWsShipBuilder.DataElements.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;

namespace WoWsShipBuilder.Core.DataUI
{
    public partial record ManeuverabilityDataContainer : DataContainerBase
    {
        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = ""FullPowerTime"", UnitKey = ""S"")]
        public decimal FullPowerForward { get; set; } = default!;

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValueUnit, GroupKey = ""FullPowerTime"", UnitKey = ""S"")]
        public decimal FullPowerBackward { get; set; } = default!;

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = ""Knots"")]
        public decimal ManeuverabilityMaxSpeed { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = ""M"")]
        public decimal ManeuverabilityTurningCircle { get; set; }

        [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = ""S"")]
        public decimal ManeuverabilityRudderShiftTime { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = ""BlastProtection"", TooltipKey = ""BlastExplanation"")]
        [DataElementVisibility(false)]
        public decimal RudderBlastProtection { get; set; }

        [DataElementType(DataElementTypes.Grouped | DataElementTypes.Tooltip, GroupKey = ""BlastProtection"", TooltipKey = ""BlastExplanation"")]
        [DataElementVisibility(false)]
        public decimal EngineBlastProtection { get; set; }
    }
}
";

        var compilation = CreateCompilation(baseCompilationInput, code);
        var driver = rawDriver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        //diagnostics.Should().NotBeEmpty();
        diagnostics.Should().BeEmpty();
    }

    private static Compilation CreateCompilation(params string[] source)
    {
        List<SyntaxTree> syntaxTrees = source.Select(sourceFile => CSharpSyntaxTree.ParseText(sourceFile)).ToList();

        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return CSharpCompilation.Create("compilation",
            syntaxTrees,
            new[]
            {
                MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(typeof(DataElementTypeAttribute).GetTypeInfo().Assembly.Location),
            },
            new(OutputKind.ConsoleApplication));
    }

}
