using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.Core.DataUI.DataElements;
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

    [Test]
    public void GroupedValuesSet_DataElementHasGroup()
    {
        const string testString = "1234test";
        var testRecord = new TestDataUi1
        {
            TestGroup1 = testString,
            Test2Group1 = testString,
        };

        testRecord.UpdateData();

        testRecord.DataElements.Should().NotBeEmpty();
        testRecord.DataElements.OfType<GroupedDataElement>().Should().HaveCount(1);
        var groupedData = testRecord.DataElements.OfType<GroupedDataElement>().Single();
        groupedData.Key.Should().BeEquivalentTo("ShipStats_test1");
        groupedData.Children.Should().HaveCount(2);
    }
}
