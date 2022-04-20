using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.Data.Generator.Test.TestStructures;

namespace WoWsShipBuilder.Data.Generator.Test;

public class SourceGeneratorResultTest
{
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
}
