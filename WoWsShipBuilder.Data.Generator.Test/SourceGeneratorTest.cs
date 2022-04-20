using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
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

    private static Compilation CreateCompilation(params string[] source)
    {
        IEnumerable<SyntaxTree> syntaxTrees = source.Select(sourceFile => CSharpSyntaxTree.ParseText(sourceFile));
        return CSharpCompilation.Create("compilation",
            syntaxTrees,
            new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
            new(OutputKind.ConsoleApplication));
    }

}
