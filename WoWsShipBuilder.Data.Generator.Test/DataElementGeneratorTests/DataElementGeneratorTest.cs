using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using WoWsShipBuilder.Data.Generator.DataElementGenerator;
using WoWsShipBuilder.DataElements.DataElements;

namespace WoWsShipBuilder.Data.Generator.Test.DataElementGeneratorTests;

[TestFixture]
[SuppressMessage("Maintainability", "S2699", Justification = "false-positive since sonarlint does not recognize custom CreateTest method")]
public partial class DataElementGeneratorTest
{
    private static CSharpSourceGeneratorTest<DataElementGenerator.DataElementGenerator, NUnitVerifier> CreateTest(string source, string expected)
    {
        const string baseClass = """
                                 namespace WoWsShipBuilder.DataElements.DataElements;

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
            TestState =
            {
                Sources = { baseClass, source },
                GeneratedSources =
                {
                    (typeof(DataElementGenerator.DataElementGenerator), "DataElementTypes.g.cs", AttributeHelper.DataElementTypesEnum),
                    (typeof(DataElementGenerator.DataElementGenerator), "TextKind.g.cs", AttributeHelper.DataElementTextKindEnum),
                    (typeof(DataElementGenerator.DataElementGenerator), "DataContainerAttribute.g.cs", AttributeHelper.DataContainerAttribute),
                    (typeof(DataElementGenerator.DataElementGenerator), "DataElementTypeAttribute.g.cs", AttributeHelper.DataElementTypeAttribute),
                    (typeof(DataElementGenerator.DataElementGenerator), "DataElementFilteringAttribute.g.cs", AttributeHelper.DataElementFilteringAttribute),
                    (typeof(DataElementGenerator.DataElementGenerator), "TestRecord.g.cs", expected),
                },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net70,
                AdditionalReferences = { MetadataReference.CreateFromFile(typeof(IDataElement).GetTypeInfo().Assembly.Location) },
            },
        };
    }
}
