using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataElements.DataElements;

namespace WoWsShipBuilder.Common.DataContainers;

public partial record AuraDataDataContainer : DataContainerBase
{
    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "KM")]
    public decimal Range { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "DPS")]
    public decimal ConstantDamage { get; set; }

    [DataElementType(DataElementTypes.KeyValue)]
    public string Flak { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValue)]
    public decimal FlakDamage { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public int HitChance { get; set; }

    public void UpdateData()
    {
        UpdateDataElements();
    }
}
