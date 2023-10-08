﻿using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using WoWsShipBuilder.Data.Generator.PropertyChangedGenerator;

namespace WoWsShipBuilder.Data.Generator.Test.PropertyChangedGeneratorTests;

[TestFixture]
public class PropertyChangedGeneratorTest
{
    private const string AttributeClass = """
                                          namespace WoWsShipBuilder.Infrastructure.Utility;

                                          [global::System.AttributeUsage(global::System.AttributeTargets.Field)]
                                          public class ObservableAttribute : global::System.Attribute
                                          {
                                              public enum Visibility
                                              {
                                                  Private, Protected, Internal, Public,
                                              }

                                              public Visibility SetterVisibility { get; set; } = Visibility.Public;

                                              public string[]? Dependants { get; set; }
                                          }
                                          """;

    [Test]
    public async Task GenerateCode_NoFields_NoCode()
    {
        var source = """
                     using WoWsShipBuilder.Infrastructure.Utility;

                     namespace Test;

                     public partial class TestViewModel
                     {
                     }

                     """;

        await new CSharpSourceGeneratorTest<PropertyChangedSourceGenerator, NUnitVerifier>
        {
            TestState =
            {
                Sources = { source, AttributeClass },
            },
        }.RunAsync();
    }

    [Test]
    public async Task GenerateCode_OneField_Success()
    {
        var source = """
                     #nullable enable
                     using WoWsShipBuilder.Infrastructure.Utility;

                     namespace Test;

                     public partial class TestViewModel
                     {
                         [Observable]
                         private string test = default!;
                     }
                     """;

        var expected = """
                       // <auto-generated />
                       #nullable enable
                       namespace Test
                       {
                           public partial class TestViewModel
                           {
                               public string Test
                               {
                                   get => this.test;
                                   set => global::ReactiveUI.IReactiveObjectExtensions.RaiseAndSetIfChanged(this, ref this.test, value);
                               }
                           }
                       }

                       """;

        var expectedDiagnostic = DiagnosticResult.CompilerError("CS0400")
            .WithSpan(@"WoWsShipBuilder.Data.Generator\WoWsShipBuilder.Data.Generator.PropertyChangedGenerator.PropertyChangedSourceGenerator\Test.TestViewModel_test.g.cs", 10, 28, 10, 38)
            .WithArguments("ReactiveUI");

        await new CSharpSourceGeneratorTest<PropertyChangedSourceGenerator, NUnitVerifier>
        {
            TestState =
            {
                Sources = { source, AttributeClass },
                GeneratedSources = { (typeof(PropertyChangedSourceGenerator), "Test.TestViewModel_test.g.cs", expected) },
                ExpectedDiagnostics = { expectedDiagnostic },
            },
        }.RunAsync();
    }

    [Test]
    public async Task GenerateCode_OneNullableField_Success()
    {
        var source = """
                     #nullable enable
                     using WoWsShipBuilder.Infrastructure.Utility;

                     namespace Test;

                     public partial class TestViewModel
                     {
                         [Observable]
                         private string? test;
                     }
                     """;

        var expected = """
                       // <auto-generated />
                       #nullable enable
                       namespace Test
                       {
                           public partial class TestViewModel
                           {
                               public string? Test
                               {
                                   get => this.test;
                                   set => global::ReactiveUI.IReactiveObjectExtensions.RaiseAndSetIfChanged(this, ref this.test, value);
                               }
                           }
                       }

                       """;

        var expectedDiagnostic = DiagnosticResult.CompilerError("CS0400")
            .WithSpan(@"WoWsShipBuilder.Data.Generator\WoWsShipBuilder.Data.Generator.PropertyChangedGenerator.PropertyChangedSourceGenerator\Test.TestViewModel_test.g.cs", 10, 28, 10, 38)
            .WithArguments("ReactiveUI");

        await new CSharpSourceGeneratorTest<PropertyChangedSourceGenerator, NUnitVerifier>
        {
            TestState =
            {
                Sources = { source, AttributeClass },
                GeneratedSources = { (typeof(PropertyChangedSourceGenerator), "Test.TestViewModel_test.g.cs", expected) },
                ExpectedDiagnostics = { expectedDiagnostic },
            },
        }.RunAsync();
    }

    [Test]
    public async Task GenerateCode_OneNullableFieldValueType_Success()
    {
        var source = """
                     #nullable enable
                     using WoWsShipBuilder.Infrastructure.Utility;

                     namespace Test;

                     public partial class TestViewModel
                     {
                         [Observable]
                         private int? test;
                     }
                     """;

        var expected = """
                       // <auto-generated />
                       #nullable enable
                       namespace Test
                       {
                           public partial class TestViewModel
                           {
                               public int? Test
                               {
                                   get => this.test;
                                   set => global::ReactiveUI.IReactiveObjectExtensions.RaiseAndSetIfChanged(this, ref this.test, value);
                               }
                           }
                       }

                       """;

        var expectedDiagnostic = DiagnosticResult.CompilerError("CS0400")
            .WithSpan(@"WoWsShipBuilder.Data.Generator\WoWsShipBuilder.Data.Generator.PropertyChangedGenerator.PropertyChangedSourceGenerator\Test.TestViewModel_test.g.cs", 10, 28, 10, 38)
            .WithArguments("ReactiveUI");

        await new CSharpSourceGeneratorTest<PropertyChangedSourceGenerator, NUnitVerifier>
        {
            TestState =
            {
                Sources = { source, AttributeClass },
                GeneratedSources = { (typeof(PropertyChangedSourceGenerator), "Test.TestViewModel_test.g.cs", expected) },
                ExpectedDiagnostics = { expectedDiagnostic },
            },
        }.RunAsync();
    }

    [Test]
    public async Task GenerateCode_ListWithNullableReferenceType_Success()
    {
        var source = """
                     #nullable enable
                     using System.Collections.Generic;
                     using WoWsShipBuilder.Infrastructure.Utility;

                     namespace Test;

                     public partial class TestViewModel
                     {
                         [Observable]
                         private List<string?> test = default!;
                     }
                     """;

        var expected = """
                       // <auto-generated />
                       #nullable enable
                       namespace Test
                       {
                           public partial class TestViewModel
                           {
                               public global::System.Collections.Generic.List<string?> Test
                               {
                                   get => this.test;
                                   set => global::ReactiveUI.IReactiveObjectExtensions.RaiseAndSetIfChanged(this, ref this.test, value);
                               }
                           }
                       }

                       """;

        var expectedDiagnostic = DiagnosticResult.CompilerError("CS0400")
            .WithSpan(@"WoWsShipBuilder.Data.Generator\WoWsShipBuilder.Data.Generator.PropertyChangedGenerator.PropertyChangedSourceGenerator\Test.TestViewModel_test.g.cs", 10, 28, 10, 38)
            .WithArguments("ReactiveUI");

        await new CSharpSourceGeneratorTest<PropertyChangedSourceGenerator, NUnitVerifier>
        {
            TestState =
            {
                Sources = { source, AttributeClass },
                GeneratedSources = { (typeof(PropertyChangedSourceGenerator), "Test.TestViewModel_test.g.cs", expected) },
                ExpectedDiagnostics = { expectedDiagnostic },
            },
        }.RunAsync();
    }
}
