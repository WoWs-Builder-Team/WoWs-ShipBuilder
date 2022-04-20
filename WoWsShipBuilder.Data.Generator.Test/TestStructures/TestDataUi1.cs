using System.Collections.Generic;
using WoWsShipBuilder.Core.DataUI.DataElements;
using WoWsShipBuilder.DataElements.DataElementAttributes;

namespace WoWsShipBuilder.Data.Generator.Test.TestStructures;

public partial record TestDataUi1 : IDataUi
{
    public List<IDataElement> DataElements { get; } = new();

    [DataElementType(DataElementTypes.Value)]
    public string TestValue { get; init; }

    public void UpdateData()
    {
        UpdateDataElements();
    }
}
