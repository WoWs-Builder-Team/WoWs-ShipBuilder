using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using NUnit.Framework;
using WoWsShipBuilder.Data.Generator.Attributes;
using WoWsShipBuilder.DataElements.DataElements;

namespace WoWsShipBuilder.Data.Generator.Test.DataElementGeneratorTest;

[TestFixture]
public partial class DataElementGeneratorTest
{
    private static CSharpSourceGeneratorTest<DataElementGenerator, NUnitVerifier> CreateTest(string source, string expected)
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
                    (typeof(DataElementGenerator), "DataElementTypes.g.cs", AttributeHelper.DataElementTypesEnum),
                    (typeof(DataElementGenerator), "DataContainerAttribute.g.cs", AttributeHelper.DataContainerAttribute),
                    (typeof(DataElementGenerator), "DataElementTypeAttribute.g.cs", AttributeHelper.DataElementTypeAttribute),
                    (typeof(DataElementGenerator), "DataElementFilteringAttribute.g.cs", AttributeHelper.DataElementFilteringAttribute),
                    (typeof(DataElementGenerator), "TestRecord.g.cs", expected),
                },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net70,
                AdditionalReferences = { MetadataReference.CreateFromFile(typeof(IDataElement).GetTypeInfo().Assembly.Location) },
            },
        };
    }
}
