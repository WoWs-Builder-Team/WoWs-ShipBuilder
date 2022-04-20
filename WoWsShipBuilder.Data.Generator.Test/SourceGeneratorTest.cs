using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.Data.Generator.Test.TestStructures;

namespace WoWsShipBuilder.Data.Generator.Test;

public class SourceGeneratorTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void SingleDataValue_DataElementsNotEmpty()
    {
        const string testString = "1234test";
        var testRecord = new TestDataUi1
        {
            TestValue = testString,
        };

        testRecord.UpdateData();

        testRecord.DataElements.Should().NotBeEmpty();
    }

    // [Test]
//     public void Test1()
//     {
//         var inputCompilation = CSharpCompilation.Create("compilation",
//             new[] { CSharpSyntaxTree.ParseText(@"namespace Test;
// public interface IDataUi
//     {
//
//     }
//
//     public partial record DataTest(string Test) : IDataUi;
//
//     public partial record DataTest2(string Test2) : IDataUi
//     {
//         [DataElementType(1)]
//     public string AttributeProp { get; }
//
//         [JsonPropertyName(""test2"")]
//     public string AttributeProp2 { get; }
//
//         [JsonPropertyName(""test2"")]
//     public string AttributeProp3 { get; }
// }
// ") },
//             new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
//             new CSharpCompilationOptions(OutputKind.ConsoleApplication));
//
//         var generator = new DataElementSourceGenerator();
//         GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
//         driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
//     }
}
