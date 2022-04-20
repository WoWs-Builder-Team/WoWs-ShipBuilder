using System.Collections.Generic;
using WoWsShipBuilder.Core.DataUI.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;

namespace WoWsShipBuilder.Data.Generator.Test.TestStructures;

public partial record TestDataUi1 : IDataUi
{
    public List<IDataElement> DataElements { get; } = new();

    [DataElementType(DataElementTypes.Value)]
    public string TestValue { get; init; }

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal TestKeyValue { get; init; }

    [DataElementType(DataElementTypes.KeyValueUnit)]
    [DataElementUnit("mm")]
    public string TestKeyUnitValue { get; init; }

    [DataElementType(DataElementTypes.Tooltip)]
    [DataElementTooltip("testTooltip")]
    public decimal TestTooltipValue { get; init; }

    [DataElementType(DataElementTypes.Grouped)]
    [DataElementGroup("test1")]
    [DataElementType(DataElementTypes.KeyValue)]
    public string TestGroup1 { get; init; }

    [DataElementType(DataElementTypes.Grouped)]
    [DataElementGroup("test1")]
    [DataElementType(DataElementTypes.Value)]
    public string Test2Group1 { get; init; }

    public void UpdateData()
    {
        UpdateDataElements();
    }
}
