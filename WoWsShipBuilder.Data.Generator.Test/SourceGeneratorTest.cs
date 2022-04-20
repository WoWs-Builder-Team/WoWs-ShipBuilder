using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using WoWsShipBuilder.Data.Generator.Attributes;
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
        var code = @"using System.Collections.Generic;
using WoWsShipBuilder.Core.DataUI.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;

namespace WoWsShipBuilder.Data.Generator.Test.TestStructures;

public partial record TestDataUi1 : IDataUi
{
    public List<IDataElement> DataElements { get; } = new();

    [DataElementType(DataElementTypes.KeyValue)]
    public string TestValue { get; init; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal TestKeyValue { get; init; }

    [DataElementType(DataElementTypes.KeyValueUnit)]
    [DataElementUnit(""mm"")]

    public string TestKeyUnitValue { get; init; } = default!;

    [DataElementType(DataElementTypes.Tooltip)]
    [DataElementTooltip(""testTooltip"")]
    public decimal TestTooltipValue { get; init; }

    [DataElementType(DataElementTypes.Grouped)]
    [DataElementGroup(""test1"")]
    [DataElementType(DataElementTypes.KeyValue)]
    public string TestGroup1 { get; init; } = default!;

    [DataElementType(DataElementTypes.Grouped)]
    [DataElementGroup(""test1"")]
    [DataElementType(DataElementTypes.KeyValue)]
    public string Test2Group1 { get; init; } = default!;

    public void UpdateData()
    {
        UpdateDataElements();
    }
}

";
        var compilation = CreateCompilation(baseCompilationInput, code);
        var driver = rawDriver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // diagnostics.Should().NotBeEmpty();
        diagnostics.Should().BeEmpty();
    }

    private static Compilation CreateCompilation(params string[] source)
    {
        List<SyntaxTree> syntaxTrees = source.Select(sourceFile => CSharpSyntaxTree.ParseText(sourceFile)).ToList();
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(AttributeGenerator.DataElementGroupAttribute));
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(AttributeGenerator.DataElementTooltipAttribute));
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(AttributeGenerator.DataElementTypeAttribute));
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(AttributeGenerator.DataElementUnitAttribute));
        syntaxTrees.Add(CSharpSyntaxTree.ParseText(AttributeGenerator.DataElementTypesEnum));

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
            },
            new(OutputKind.ConsoleApplication));
    }

}
