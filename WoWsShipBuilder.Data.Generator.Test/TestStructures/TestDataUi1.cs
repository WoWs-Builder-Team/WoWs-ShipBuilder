using WoWsShipBuilder.DataElements.DataElementAttributes;

namespace WoWsShipBuilder.Data.Generator.Test.TestStructures;

public partial record TestDataUi1 : ProjectileDataContainer
{
    [DataElementType(DataElementTypes.Value)]
    public string TestValue { get; init; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    [DataElementFiltering(false)]
    public decimal TestKeyValue { get; init; }

    [DataElementType(DataElementTypes.KeyValue)]
    [DataElementFiltering(true, "TestVisibility")]
    public decimal TestVisibilityCustom { get; init; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "mm")]
    public string TestKeyUnitValue { get; init; } = default!;

    [DataElementType(DataElementTypes.Tooltip, TooltipKey = "testTooltip")]
    public decimal TestTooltipValue { get; init; }

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "test1")]
    public string TestGroup1 { get; init; } = default!;

    [DataElementType(DataElementTypes.Grouped | DataElementTypes.KeyValue, GroupKey = "test1")]
    public string Test2Group1 { get; init; } = default!;

    public void UpdateData()
    {
        UpdateDataElements();
    }

    public bool TestVisibility(object value)
    {
        return true;
    }
}
