using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.CSharp.Testing.NUnit;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using WoWsShipBuilder.Data.Generator.DataElementGenerator;
using WoWsShipBuilder.DataElements;

namespace WoWsShipBuilder.Data.Generator.Test.DataElementAnalyzerTests;

using Verify = AnalyzerVerifier<DataElementAnalyzer>;

[TestFixture]
public partial class DataElementAnalyzerTest
{
    [Test]
    public async Task Analyze_InvalidDataElementType_Diagnostics()
    {
        var source = """
                     using WoWsShipBuilder.DataElements.DataElementAttributes;
                     using WoWsShipBuilder.DataElements;

                     namespace Test;

                     [DataContainer]
                     public partial record TestRecord : DataContainerBase
                     {
                         [DataElementType((DataElementTypes)128)]
                         public decimal {|SB0001:Prop1|} { get; set; }
                     }
                     """;

        await CreateTest(source).RunAsync();
    }

    private static CSharpAnalyzerTest<DataElementAnalyzer, NUnitVerifier> CreateTest(string source)
    {
        const string baseClass = """
                                 namespace WoWsShipBuilder.DataElements;

                                 public abstract record DataContainerBase
                                 {
                                     public global::System.Collections.Generic.List<IDataElement> DataElements { get; } = new();

                                     protected static bool ShouldAdd(object? value)
                                     {
                                         return value switch
                                         {
                                             string strValue => !string.IsNullOrEmpty(strValue),
                                             decimal decValue => decValue != 0,
                                             (decimal min, decimal max) => min > 0 || max > 0,
                                             int intValue => intValue != 0,
                                             _ => false,
                                         };
                                     }
                                 }
                                 """;
        return new()
        {
            TestCode = source,
            TestState =
            {
                Sources =
                {
                    AttributeHelper.DataElementTypesEnum,
                    AttributeHelper.DataElementTextKindEnum,
                    AttributeHelper.DataContainerAttribute,
                    AttributeHelper.DataElementTypeAttribute,
                    AttributeHelper.DataElementFilteringAttribute,
                    baseClass,
                },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net70,
                AdditionalReferences = { MetadataReference.CreateFromFile(typeof(IDataElement).GetTypeInfo().Assembly.Location) },
            },
        };
    }
}
